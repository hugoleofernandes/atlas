using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities;

public sealed class IdentityAuditLog : AuditLogBase
{
    public Guid Id { get; private set; }

    public IdentityAuditLog()
    {
        Id = Guid.NewGuid();
    }

    public IdentityAuditLog(
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