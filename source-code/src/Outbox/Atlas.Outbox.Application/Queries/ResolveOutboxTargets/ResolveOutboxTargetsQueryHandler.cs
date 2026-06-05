using Atlas.Outbox.Contracts.Targets;

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

        var targets = (await catalogReader.ReadAsync(eventType, ct))
            .OrderBy(target => target.Order)
            .ThenBy(target => target.Name, StringComparer.Ordinal)
            .ToList();

        var duplicateTargets = targets
            .GroupBy(target => target.Name, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key} [{string.Join(", ", group.Select(target => target.Mode).Distinct())}]")
            .ToList();

        if (duplicateTargets.Count > 0)
        {
            throw new InvalidOperationException(
                $"Duplicate target mapping detected for '{eventType.Name}': {string.Join("; ", duplicateTargets)}."
            );
        }

        return targets;
    }
}
