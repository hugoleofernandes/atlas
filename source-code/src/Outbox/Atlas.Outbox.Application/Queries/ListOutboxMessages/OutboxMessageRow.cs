namespace Atlas.Outbox.Application.Queries.ListOutboxMessages;

/// <summary>
/// One attempt row, flat — the frontend groups by IdempotencyKey client-side.
/// Status is derived: Processed | DeadLettered | Failed | Pending.
/// </summary>
public sealed record OutboxMessageRow(
    Guid Id,
    Guid ModuleId,
    string ModuleName,
    Guid IdempotencyKey,
    Guid? ParentOutboxMessageId,
    int AttemptNumber,
    string Name,
    DateTime OccurredOn,
    string Status,
    string Origin,
    string? Error,
    DateTime? ProcessedOn,
    DateTime? FailedAt,
    DateTime? DeadLetteredOn,
    Guid TenantId,
    string? UserEmail,
    string CorrelationId,
    string? ResubmittedByEmail,
    bool HasReplayChild,
    int ExecutionCount,
    IReadOnlyList<OutboxHandlerExecutionDetail> Executions
)
{
    public string NormalizedName => NormalizeEventName(Name);

    private static string NormalizeEventName(string name)
    {
        if (name.EndsWith("IntegrationEvent", StringComparison.Ordinal))
            return name[..^"IntegrationEvent".Length];

        if (name.EndsWith("DomainEvent", StringComparison.Ordinal))
            return name[..^"DomainEvent".Length];

        return name;
    }
}
