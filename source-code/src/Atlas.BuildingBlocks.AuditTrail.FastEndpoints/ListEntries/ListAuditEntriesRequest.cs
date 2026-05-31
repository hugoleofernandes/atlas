namespace Atlas.BuildingBlocks.AuditTrail.FastEndpoints.ListEntries;

public sealed record ListAuditEntriesRequest(
    Guid      EntityTypeId,
    DateTime? From,
    DateTime? To,
    string?   Action,
    string?   EntityId);
