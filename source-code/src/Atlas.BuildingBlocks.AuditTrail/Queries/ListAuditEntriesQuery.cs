namespace Atlas.BuildingBlocks.AuditTrail.Queries;

public sealed record ListAuditEntriesQuery(
    Guid      EntityTypeId,
    DateTime? From,
    DateTime? To,
    string?   Action,
    string?   EntityId);
