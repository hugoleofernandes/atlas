using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.IntegrationEvents;

public sealed class IntegrationEventRegistry : IIntegrationEventRegistry
{
    public OutboxEventDefinition? Resolve(DomainEvent e, IOutboxMappings outboxMappings)
    {
        var type = e.GetType();

        var map = outboxMappings.All.ToDictionary(x => x.GetType());

        if (!map.TryGetValue(type, out var cfg))
            return null;

        return new OutboxEventDefinition(
            Name: cfg.Name ?? ToDefaultName(type),
            Type: type,
            Module: cfg.Module ?? ToDefaultModule(type)
        );
    }

    public IEnumerable<IntegrationEventMapping> ResolveAll(IEnumerable<DomainEvent> events, IOutboxMappings outboxMappings)
    {
        var map = outboxMappings.All.ToDictionary(x => x.Type);

        foreach (var e in events)
        {
            var type = e.GetType();

            if (!map.TryGetValue(type, out var cfg))
                continue;

            yield return new IntegrationEventMapping(
                e,
                new OutboxEventDefinition(
                    Name: cfg.Name ?? ToDefaultName(type),
                    Type: type,
                    Module: cfg.Module ?? ToDefaultModule(type)
                )
            );
        }
    }

    private static string ToDefaultName(Type t)
        => ToKebabCase(t.Name).Replace("domain-event", "");

    private static string ToDefaultModule(Type t)
        => t.Namespace?.Split('.').Skip(2).FirstOrDefault() ?? "default";

    private static string ToKebabCase(string input)
        => string.Concat(input.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "-" + char.ToLower(c) : char.ToLower(c).ToString()));
}