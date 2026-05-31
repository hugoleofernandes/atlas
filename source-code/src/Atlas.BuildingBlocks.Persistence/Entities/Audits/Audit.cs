using Atlas.SharedKernel.Domain;

namespace Atlas.BuildingBlocks.Persistence.Entities.Audits;

public sealed class Audit : IMultiTenantEntity, INotAuditable
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid EntityTypeId { get; private set; }
    public string Action { get; private set; } = default!;
    public string? EntityId { get; private set; }
    public string? UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public string ChangesJson { get; private set; } = default!;
    public DateTime OccurredAtUtc { get; private set; }

    public void Initialize(
        Guid    entityTypeId,
        string  action,
        string? entityId,
        string? userId,
        Guid    tenantId,
        string  changesJson)
    {
        EntityTypeId  = entityTypeId;
        Action        = action;
        EntityId      = entityId;
        UserId        = userId;
        TenantId      = tenantId;
        ChangesJson   = changesJson;
        OccurredAtUtc = DateTime.UtcNow;
    }

    public void SetTenantId(Guid tenantId) => TenantId = tenantId;
}
