namespace Atlas.BuildingBlocks.AuditTrail.Queries;

public sealed record AuditEntryDto(
    Guid     Id,
    Guid     EntityTypeId,
    string   Action,
    string?  EntityId,
    string?  UserId,
    DateTime OccurredAtUtc,
    string   ChangesJson);
