using Atlas.SharedKernel.Domain;

namespace Atlas.BuildingBlocks.Persistence.Audits;

/// <summary>
/// Shared audit log entity used across all modules.
/// Each module maps it to its own schema via EF configuration.
/// Implements INotAuditable — audit logs should not audit themselves.
/// </summary>
public sealed class Audit : AuditLogBase, INotAuditable
{
    public Guid Id { get; private set; }

    public Audit()
    {
        Id = Guid.NewGuid();
    }
}
