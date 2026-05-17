namespace Atlas.SharedKernel.Domain;

/// <summary>
/// Base class for aggregate roots that carry audit metadata.
/// Combines AggregateRootBase (domain events) with IAuditableEntity (audit fields).
///
/// Fields are automatically populated by MultiTenantDbContext on SaveChangesAsync.
/// </summary>
public abstract class AuditableAggregateRoot : AggregateRootBase, IAuditableEntity
{
    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public string? CreatedByEmail { get; private set; }

    public DateTime? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public string? UpdatedByEmail { get; private set; }

    public void SetCreated(DateTime at, Guid? by, string? byEmail)
    {
        CreatedAt = at;
        CreatedBy = by;
        CreatedByEmail = byEmail;
    }

    public void SetUpdated(DateTime at, Guid? by, string? byEmail)
    {
        UpdatedAt = at;
        UpdatedBy = by;
        UpdatedByEmail = byEmail;
    }
}
