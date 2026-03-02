namespace Atlas.SharedKernel.Domain;

public abstract class AuditLogBase : IAuditLog, IMultiTenantEntity
{
    public string EntityName { get; protected set; } = default!;
    public string Action { get; protected set; } = default!;
    public string? EntityId { get; protected set; }
    public string? UserId { get; protected set; }

    public Guid TenantId { get; protected set; }

    public string ChangesJson { get; protected set; } = default!;

    public DateTime OccurredAtUtc { get; protected set; }


    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public void Initialize(
        string entityName,
        string action,
        string? entityId,
        string? userId,
        Guid tenantId,
        string changesJson)
    {
        EntityName = entityName;
        Action = action;
        EntityId = entityId;
        UserId = userId;
        TenantId = tenantId;
        ChangesJson = changesJson;
        OccurredAtUtc = DateTime.UtcNow;
    }

}