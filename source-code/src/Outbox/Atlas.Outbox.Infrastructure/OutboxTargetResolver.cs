using Atlas.Outbox.Application.Queries.ResolveOutboxTargets;
using Atlas.Outbox.Application.Targets;
using Atlas.Outbox.Domain.Targets;

namespace Atlas.Outbox.Infrastructure;

internal sealed class OutboxTargetResolver(IEnumerable<ITargetCatalog> catalogs) : ITargetCatalogReader
{
    public Task<IReadOnlyList<TargetMapping>> ReadAsync(Type eventType, CancellationToken ct)
    {
        var targets = catalogs.SelectMany(catalog => catalog.GetFor(eventType)).ToList();

        return Task.FromResult<IReadOnlyList<TargetMapping>>(targets);
    }
}
