namespace Atlas.Outbox.Application.Queries.ResolveOutboxTargets;

public interface IIntegrationEventTypeResolver
{
    Type? Resolve(string eventTypeName);
}
