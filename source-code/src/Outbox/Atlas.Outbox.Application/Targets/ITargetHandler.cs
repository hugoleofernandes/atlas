using Atlas.BuildingBlocks.Outbox.ListPendingMessages;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.Targets;

public interface ITargetHandler
{
    string Name { get; }

    Task<HandlerInvocationResult> ExecuteAsync(ListPendingMessagesDto message, CancellationToken ct);
}
