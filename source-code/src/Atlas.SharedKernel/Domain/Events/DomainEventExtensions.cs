namespace Atlas.SharedKernel.Domain.Events;

public static class DomainEventExtensions
{
    public static bool HasEvent<TEvent>(
    this IEnumerable<IDomainEvent> events)
    where TEvent : IDomainEvent
    {
        return events.OfType<TEvent>().Any();
    }

    public static TEvent? GetEvent<TEvent>(
        this IEnumerable<IDomainEvent> events)
        where TEvent : class, IDomainEvent
    {
        return events.OfType<TEvent>().FirstOrDefault();
    }

    public static IEnumerable<TEvent> GetEvents<TEvent>(
        this IEnumerable<IDomainEvent> events)
        where TEvent : IDomainEvent
    {
        return events.OfType<TEvent>();
    }
}

