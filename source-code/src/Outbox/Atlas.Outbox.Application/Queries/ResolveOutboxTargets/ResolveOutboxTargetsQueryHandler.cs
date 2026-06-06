using Atlas.Outbox.Domain.Targets;

namespace Atlas.Outbox.Application.Queries.ResolveOutboxTargets;

public sealed class ResolveOutboxTargetsQueryHandler(
    IIntegrationEventTypeResolver typeResolver,
    ITargetCatalogReader catalogReader
) : IResolveOutboxTargetsQueryHandler
{
    public async Task<IReadOnlyList<TargetMapping>> ExecuteAsync(ResolveOutboxTargetsQuery query, CancellationToken ct)
    {
        var eventType =
            typeResolver.Resolve(query.Message.Type)
            ?? throw new InvalidOperationException($"Integration event type '{query.Message.Type}' not found.");

        var rawTargets = await catalogReader.ReadAsync(eventType, ct);
        var targetSet = OutboxTargetSet.Create(eventType, rawTargets);

        return targetSet.Items;
    }
}
