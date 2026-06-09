namespace Atlas.BuildingBlocks.Audit.Queries;

public sealed record ListAuditEntriesQuery(
    Guid      EntityTypeId,
    DateTime? From,
    DateTime? To,
    string?   Action,
    string?   EntityId);
