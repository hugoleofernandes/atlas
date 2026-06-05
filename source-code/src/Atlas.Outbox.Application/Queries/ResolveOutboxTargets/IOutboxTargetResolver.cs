using Atlas.Outbox.Contracts;

namespace Atlas.Outbox.Application.Queries.ResolveOutboxTargets;

public interface IOutboxTargetResolver
{
    Task<IReadOnlyList<OutboxDispatchTargetDto>> ResolveAsync(
        OutboxMessageDto message,
        CancellationToken ct);
}
