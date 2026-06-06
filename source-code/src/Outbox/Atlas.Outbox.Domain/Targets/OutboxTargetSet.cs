namespace Atlas.Outbox.Domain.Targets;

public sealed class OutboxTargetSet
{
    private OutboxTargetSet(IReadOnlyList<TargetMapping> items)
    {
        Items = items;
    }

    public IReadOnlyList<TargetMapping> Items { get; }

    public static OutboxTargetSet Create(Type eventType, IReadOnlyList<TargetMapping> rawTargets)
    {
        var orderedTargets = rawTargets
            .OrderBy(target => target.Order)
            .ThenBy(target => target.Name, StringComparer.Ordinal)
            .ToList();

        var duplicateTargets = orderedTargets
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

        return new OutboxTargetSet(orderedTargets);
    }
}
