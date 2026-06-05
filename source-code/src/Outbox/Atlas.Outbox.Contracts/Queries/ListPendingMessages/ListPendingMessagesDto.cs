namespace Atlas.Outbox.Contracts.Queries.ListPendingMessages;

public sealed record ListPendingMessagesDto(
    Guid Id,
    string Name,
    string Type,
    string Payload,
    int AttemptNumber,
    Guid TenantId,
    Guid UserId,
    string? UserEmail,
    string CorrelationId,
    string? TraceParent,
    Guid IdempotencyKey,
    DateTime? LockedUntil
);
