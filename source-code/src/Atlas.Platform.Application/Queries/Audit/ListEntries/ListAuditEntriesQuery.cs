namespace Atlas.Platform.Application.Queries.Audit.ListEntries;

public sealed record ListAuditEntriesQuery(
    Guid      EntityTypeId,
    DateTime? From,
    DateTime? To,
    string?   Action,
    string?   EntityId);
