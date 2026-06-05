using Atlas.Outbox.Contracts.Queries.ListPendingMessages;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Contracts.Targets;

public interface ITargetHandler
{
    string Name { get; }

    Task<HandlerInvocationResult> ExecuteAsync(ListPendingMessagesDto message, CancellationToken ct);
}
