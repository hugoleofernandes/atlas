using Atlas.Outbox.Domain.Targets;

namespace Atlas.Outbox.Application.Queries.ResolveOutboxTargets;

public interface ITargetCatalogReader
{
    Task<IReadOnlyList<TargetMapping>> ReadAsync(Type eventType, CancellationToken ct);
}
