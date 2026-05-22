using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Dispatching;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.ProcessOutbox;

/// <summary>
/// Reads a batch of pending outbox messages, dispatches each one, and persists the outcome.
///
/// Registered twice in the worker — once per module (Identity, Staff) — each time with a
/// different IOutboxWorkerRepository and OutboxUnitOfWork pointing to that module's DbContext.
/// The handler itself has no knowledge of which module it is processing.
///
/// Context hydration: before each dispatch the scoped IRequestContextSetter is populated from
/// the OutboxMessage (TenantId, UserId, CorrelationId). This makes AuditTrailService,
/// EntityTenantStamper, EntityChangeStamper and the SavePipeline run under the correct context.
///
/// Save strategy: PersistDbDecorator calls UnitOfWork.SaveChangesAsync once after the handler
/// returns — one flush for the entire batch (parent updates + retry inserts + execution records).
///
/// Retry model — Attempt-Chain:
///   Each OutboxMessage row represents exactly one processing attempt.
///   On failure the current row is closed (FailedAt set) and a new child row
///   (AttemptNumber + 1, same IdempotencyKey) is inserted atomically in the same transaction.
///   Handlers that already succeeded skip re-execution via the idempotency check.
///   On max attempts the row is dead-lettered instead of spawning a child.
///
/// Execution history:
///   After each dispatch (success or failure), one OutboxHandlerExecution row is inserted
///   per handler into outbox_handler_executions. This gives a fully queryable, structured
///   record of every handler invocation across all attempts.
/// </summary>
public sealed class ProcessOutboxCommandHandler : IIdentityOutboxCommandHandler, IStaffOutboxCommandHandler
{
    private readonly IOutboxWorkerRepository  _repository;
    private readonly IOutboxMessageDispatcher _dispatcher;
    private readonly IDispatcherInvoker       _dispatcherInvoker;
    private readonly IUnitOfWork              _uow;
    private readonly IRequestContextSetter    _contextSetter;
    private readonly ITraceContextSetter      _traceContextSetter;

    public IUnitOfWork UnitOfWork => _uow;

    public ProcessOutboxCommandHandler(
        IOutboxWorkerRepository  repository,
        IOutboxMessageDispatcher dispatcher,
        IDispatcherInvoker       dispatcherInvoker,
        IUnitOfWork              uow,
        IRequestContextSetter    contextSetter,
        ITraceContextSetter      traceContextSetter)
    {
        _repository         = repository;
        _dispatcher         = dispatcher;
        _dispatcherInvoker  = dispatcherInvoker;
        _uow                = uow;
        _contextSetter      = contextSetter;
        _traceContextSetter = traceContextSetter;
    }

    public async Task<ProcessOutboxOutput> ExecuteAsync(ProcessOutboxCommand command, CancellationToken ct)
    {
        var messages = await _repository.GetPendingBatchAsync(
            command.BatchSize, command.LockDuration, ct);

        if (messages.Count == 0)
            return new ProcessOutboxOutput(0, 0, 0);

        var processed    = 0;
        var failed       = 0;
        var deadLettered = 0;

        foreach (var message in messages)
        {
            // Hydrate the scoped request context from the outbox message so that
            // SavePipeline (audit trail, entity stampers) and any handler that
            // resolves IRequestContext sees the correct tenant/user/correlation.
            _contextSetter.Set(message.TenantId, string.Empty, message.UserId, message.UserEmail);
            _contextSetter.SetCorrelationId(message.CorrelationId);

            // Hydrate the scoped trace context so dispatcher decorators (logging,
            // tracing) can read event type, message id, attempt and correlation
            // without depending on OutboxMessage directly.
            _traceContextSetter.Set(
                message.TraceParent,
                message.Name,
                message.Id,
                message.AttemptNumber,
                message.CorrelationId);

            try
            {
                var results  = await _dispatcherInvoker.InvokeAsync(_dispatcher, message, ct);
                var failures = results.Where(r => !r.IsSuccess).ToList();

                // Persist one structured execution row per handler — queryable history.
                var executions = BuildExecutions(message.Id, results);
                await _repository.AddExecutionsAsync(executions, ct);

                if (failures.Count == 0)
                {
                    message.MarkAsProcessed();
                    processed++;
                }
                else if (message.IsMaxAttemptReached(command.MaxRetries))
                {
                    message.MarkAsDeadLettered();
                    deadLettered++;
                }
                else
                {
                    var errorSummary = $"{failures.Count} of {results.Count} handler(s) failed on attempt {message.AttemptNumber}.";
                    var retry        = message.CreateRetryAttempt(errorSummary);
                    await _repository.AddRetryAsync(retry, ct);
                    failed++;
                }
            }
            catch (Exception ex)
            {
                // Dispatcher-level failure: unknown event type, deserialization error,
                // no handlers registered. Recorded as a single "Dispatcher" execution.
                var executions = new[]
                {
                    new OutboxHandlerExecution(message.Id, "Dispatcher", isSuccess: false, ex.Message)
                };
                await _repository.AddExecutionsAsync(executions, ct);

                if (message.IsMaxAttemptReached(command.MaxRetries))
                {
                    message.MarkAsDeadLettered();
                    deadLettered++;
                }
                else
                {
                    var retry = message.CreateRetryAttempt(ex.Message);
                    await _repository.AddRetryAsync(retry, ct);
                    failed++;
                }
            }
        }

        return new ProcessOutboxOutput(processed, failed, deadLettered);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IReadOnlyList<OutboxHandlerExecution> BuildExecutions(
        Guid outboxMessageId,
        IReadOnlyList<HandlerInvocationResult> results)
    {
        return results
            .Select(r => new OutboxHandlerExecution(
                outboxMessageId,
                r.HandlerName,
                r.IsSuccess,
                r.ErrorMessage))
            .ToList();
    }
}
