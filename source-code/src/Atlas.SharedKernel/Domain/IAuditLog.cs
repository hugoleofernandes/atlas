namespace Atlas.SharedKernel.Domain;

public interface IAuditLog
{
    string EntityName { get; }
    string Action { get; }
    string? EntityId { get; }
    string? UserId { get; }
    Guid TenantId { get; }
    string ChangesJson { get; }
    DateTime OccurredAtUtc { get; }

    void Initialize(
    string entityName,
    string action,
    string? entityId,
    string? userId,
    Guid tenantId,
    string changesJson);
}

