using Atlas.Outbox.Contracts.Queries.ListPendingMessages;
using Atlas.Outbox.Contracts.Targets;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.Commands.ProcessOutboxTargets;

public interface IOutboxTargetExecutor
{
    TargetMode Mode { get; }

    Task<HandlerInvocationResult> ExecuteAsync(
        TargetMapping target,
        ListPendingMessagesDto message,
        CancellationToken ct
    );
}
