using Atlas.Outbox.Application.Queries.ResolveOutboxTargets;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Dispatching;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.Commands.ProcessOutboxTargets;

public sealed class ProcessOutboxTargetsCommandHandler(
    IEnumerable<IOutboxTargetExecutor> executors,
    IRequestContextSetter requestContextSetter,
    ITraceContextSetter traceContextSetter)
    : IProcessOutboxTargetsCommandHandler
{
    public IUnitOfWork UnitOfWork => NullUnitOfWork.Instance;

    public async Task<IReadOnlyList<HandlerInvocationResult>> ExecuteAsync(
        ProcessOutboxTargetsCommand command,
        CancellationToken ct)
    {
        var message = command.Message;

        requestContextSetter.Set(message.TenantId, string.Empty, message.UserId, message.UserEmail);
        requestContextSetter.SetCorrelationId(message.CorrelationId);

        traceContextSetter.Set(
            message.TraceParent,
            message.Name,
            message.Id,
            message.AttemptNumber,
            message.CorrelationId);

        var executorMap = executors.ToDictionary(
            executor => BuildKey(executor.Mode, executor.Name),
            StringComparer.Ordinal);

        var results = new List<HandlerInvocationResult>(command.Targets.Count);

        foreach (var target in command.Targets)
        {
            if (!executorMap.TryGetValue(BuildKey(target.Mode, target.Name), out var executor))
            {
                results.Add(HandlerInvocationResult.Failure(
                    target.Name,
                    $"No {target.Mode} executor registered for target '{target.Name}'."));
                continue;
            }

            results.Add(await executor.ExecuteAsync(message, ct));
        }

        return results;
    }

    private static string BuildKey(OutboxTargetMode mode, string name) =>
        $"{mode}:{name}";
}
