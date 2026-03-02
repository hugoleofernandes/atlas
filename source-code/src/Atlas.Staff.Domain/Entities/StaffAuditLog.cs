using Atlas.SharedKernel.Domain;

namespace Atlas.Staff.Domain.Entities;

public sealed class StaffAuditLog : AuditLogBase
{
    public Guid Id { get; private set; }

    public StaffAuditLog()
    {
        Id = Guid.NewGuid();
    }

    public StaffAuditLog(
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