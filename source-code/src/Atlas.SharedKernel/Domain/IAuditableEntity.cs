namespace Atlas.SharedKernel.Domain;

/// <summary>
/// Marker interface for entities that carry creation and modification metadata.
/// Fields are automatically populated by MultiTenantDbContext on SaveChangesAsync.
///
/// CreatedBy/UpdatedBy store the UserId for referential integrity.
/// CreatedByEmail/UpdatedByEmail store a snapshot of the email at the time of the action —
/// preserved even if the user changes their email or is deleted later.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    Guid? CreatedBy { get; }
    string? CreatedByEmail { get; }

    DateTime? UpdatedAt { get; }
    Guid? UpdatedBy { get; }
    string? UpdatedByEmail { get; }

    void SetCreated(DateTime at, Guid? by, string? byEmail);
    void SetUpdated(DateTime at, Guid? by, string? byEmail);
}
