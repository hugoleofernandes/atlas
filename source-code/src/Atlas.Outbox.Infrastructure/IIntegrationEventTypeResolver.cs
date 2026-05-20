namespace Atlas.Outbox.Infrastructure;

internal interface IIntegrationEventTypeResolver
{
    Type? Resolve(string eventTypeName);
}
