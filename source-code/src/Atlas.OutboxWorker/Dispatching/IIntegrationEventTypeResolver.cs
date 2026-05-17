namespace Atlas.OutboxWorker.Dispatching;

public interface IIntegrationEventTypeResolver
{
    Type? Resolve(string typeName);
}
