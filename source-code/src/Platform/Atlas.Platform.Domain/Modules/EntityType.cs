using Atlas.SharedKernel.Domain;

namespace Atlas.Platform.Domain.Modules;

/// <summary>
/// Represents an auditable entity type within a module.
/// Name = CLR class name stored in the audits table (e.g. "User", "Role").
/// </summary>
public sealed class EntityType : AggregateRoot, INotMultiTenant
{
    public Guid   Id       { get; private set; }
    public Guid   ModuleId { get; private set; }
    public string Name     { get; private set; } = default!;
    public bool   IsActive { get; private set; }

    private EntityType() { }

    public static EntityType Create(Guid moduleId, string name)
        => new()
        {
            Id       = Guid.NewGuid(),
            ModuleId = moduleId,
            Name     = name,
            IsActive = true,
        };

    /// <summary>
    /// Creates an EntityType with an explicit, deterministic ID.
    /// Use this overload in seeders with constants from Atlas.SharedDomain so that
    /// the frontend can reference EntityTypeIds without querying the registry at runtime.
    /// </summary>
    public static EntityType Create(Guid id, Guid moduleId, string name)
        => new()
        {
            Id       = id,
            ModuleId = moduleId,
            Name     = name,
            IsActive = true,
        };

    public void Activate()   => IsActive = true;
    public void Deactivate() => IsActive = false;
}
