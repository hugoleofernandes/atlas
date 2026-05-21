using Atlas.Outbox.Application.OutboxMessages;
using Atlas.SharedKernel.Application;
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
/// returns — one flush for the entire batch of status updates.
/// </summary>
public sealed class ProcessOutboxCommandHandler : IIdentityOutboxCommandHandler, IStaffOutboxCommandHandler
{
    private readonly IOutboxWorkerRepository _repository;
    private readonly IOutboxMessageDispatcher _dispatcher;
    private readonly IUnitOfWork _uow;
    private readonly IRequestContextSetter _contextSetter;

    public IUnitOfWork UnitOfWork => _uow;

    public ProcessOutboxCommandHandler(
        IOutboxWorkerRepository repository,
        IOutboxMessageDispatcher dispatcher,
        IUnitOfWork uow,
        IRequestContextSetter contextSetter)
    {
        _repository    = repository;
        _dispatcher    = dispatcher;
        _uow           = uow;
        _contextSetter = contextSetter;
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
            if (message.HasExceededRetries(command.MaxRetries))
            {
                message.MarkAsDeadLettered();
                deadLettered++;
                continue;
            }

            // Hydrate the scoped request context from the outbox message so that
            // SavePipeline (audit trail, entity stampers) and any handler that
            // resolves IRequestContext sees the correct tenant/user/correlation.
            _contextSetter.Set(message.TenantId, string.Empty, message.UserId, null);
            _contextSetter.SetCorrelationId(message.CorrelationId);

            try
            {
                await _dispatcher.DispatchAsync(message, ct);
                message.MarkAsProcessed();
                processed++;
            }
            catch (Exception ex)
            {
                message.MarkAsFailed(ex.Message);
                failed++;

                if (message.HasExceededRetries(command.MaxRetries))
                {
                    message.MarkAsDeadLettered();
                    deadLettered++;
                }
            }
        }

        return new ProcessOutboxOutput(processed, failed, deadLettered);
    }
}
