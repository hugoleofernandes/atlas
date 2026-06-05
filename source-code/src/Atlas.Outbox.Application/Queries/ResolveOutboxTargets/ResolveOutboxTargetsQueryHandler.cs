namespace Atlas.Outbox.Application.Queries.ResolveOutboxTargets;

public sealed class ResolveOutboxTargetsQueryHandler(IOutboxTargetResolver resolver)
    : IResolveOutboxTargetsQueryHandler
{
    public Task<IReadOnlyList<OutboxDispatchTargetDto>> ExecuteAsync(
        ResolveOutboxTargetsQuery query,
        CancellationToken ct) =>
        resolver.ResolveAsync(query.Message, ct);
}
