namespace Atlas.BuildingBlocks.Audit.Queries;

public sealed record ListAuditEntriesRequest(
    Guid      EntityTypeId,
    DateTime? From,
    DateTime? To,
    string?   Action,
    string?   EntityId);
