//namespace Atlas.BuildingBlocks.Audit;

//public sealed class AuditEntry
//{
//    public Guid Id { get; private set; } = Guid.NewGuid();

//    public string Action { get; private set; } = default!;
//    public string EntityName { get; private set; } = default!;
//    public string? EntityId { get; private set; }

//    public string? UserId { get; private set; }
//    public string? TenantId { get; private set; }

//    public string? Changes { get; private set; }

//    public DateTime OccurredAtUtc { get; private set; } = DateTime.UtcNow;

//    private AuditEntry() { }

//    public AuditEntry(
//        string action,
//        string entityName,
//        string? entityId,
//        string? userId,
//        string? tenantId,
//        string? changes)
//    {
//        Action = action;
//        EntityName = entityName;
//        EntityId = entityId;
//        UserId = userId;
//        TenantId = tenantId;
//        Changes = changes;
//    }
//}

//todo:deletar