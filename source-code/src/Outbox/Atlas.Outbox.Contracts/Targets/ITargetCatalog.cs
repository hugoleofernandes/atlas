namespace Atlas.Outbox.Contracts.Targets;

public interface ITargetCatalog
{
    IReadOnlyList<TargetMapping> GetFor(Type eventType);
}
