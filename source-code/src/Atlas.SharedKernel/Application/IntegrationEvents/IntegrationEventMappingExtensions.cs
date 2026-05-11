using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.IntegrationEvents;

public static class IntegrationEventMappingExtensions
{
    public static IReadOnlyList<IntegrationEventMapping> GetEvents<TEvent>(
        this IEnumerable<IntegrationEventMapping> mappings)
        where TEvent : DomainEvent
    {
        return mappings
            .Where(x => x.Event is TEvent)
            .ToList();
    }
}