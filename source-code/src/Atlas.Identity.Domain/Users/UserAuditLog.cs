using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Users;

/// <summary>
/// Represents an audit log entry for the Identity module.
///
/// Purpose:
/// - Tracks changes to entities for observability and traceability.
///
/// Invariants:
/// - Each log entry must contain the entity name, action, and timestamp.
/// - TenantId must always be present for multi-tenant isolation.
///
/// Design Decisions:
/// - Uses JSON to store change details for flexibility.
/// - Stored per module to maintain bounded context isolation.
///
/// Boundaries:
/// - Does not enforce business rules.
/// - Used for auditing and diagnostics only.
/// </summary>
public sealed class UserAuditLog : AuditLogBase
{
    public Guid Id { get; private set; }

    public UserAuditLog()
    {
        Id = Guid.NewGuid();
    }

    public UserAuditLog(
        string entityName,
        string action,
        string? entityId,
        string? userId,
        Guid tenantId,
        string changesJson)
    {
        Id = Guid.NewGuid();
        OccurredAtUtc = DateTime.UtcNow;

        EntityName = entityName;
        Action = action;
        EntityId = entityId;
        UserId = userId;
        TenantId = tenantId;
        ChangesJson = changesJson;
    }
}