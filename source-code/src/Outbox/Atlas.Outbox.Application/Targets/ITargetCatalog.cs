using Atlas.Outbox.Domain.Targets;

namespace Atlas.Outbox.Application.Targets;

public interface ITargetCatalog
{
    IReadOnlyList<TargetMapping> GetFor(Type eventType);
}
