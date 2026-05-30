namespace Atlas.Platform.API.Endpoints.Audit.ListEntries;

public sealed record ListAuditEntriesRequest(
    Guid      EntityTypeId,
    DateTime? From,
    DateTime? To,
    string?   Action,
    string?   EntityId);
