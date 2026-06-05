using Atlas.Outbox.Application.Queries.ResolveOutboxTargets;
using Atlas.Outbox.Contracts;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.Commands.ProcessOutboxTargets;

public interface IOutboxTargetExecutor
{
    string Name { get; }
    OutboxTargetMode Mode { get; }

    Task<HandlerInvocationResult> ExecuteAsync(
        OutboxMessageDto message,
        CancellationToken ct);
}
