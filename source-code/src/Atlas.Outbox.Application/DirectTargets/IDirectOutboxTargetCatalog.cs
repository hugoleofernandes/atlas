namespace Atlas.Outbox.Application.DirectTargets;

public interface IDirectOutboxTargetCatalog
{
    IReadOnlyList<DirectOutboxTargetDefinition> GetFor(Type eventType);
}
