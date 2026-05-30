namespace Atlas.Platform.Application.Queries.Audit.ListEntries;

public sealed record AuditEntryDto(
    Guid     Id,
    string   EntityName,
    string   Action,
    string?  EntityId,
    string?  UserId,
    DateTime OccurredAtUtc,
    string   ChangesJson);
