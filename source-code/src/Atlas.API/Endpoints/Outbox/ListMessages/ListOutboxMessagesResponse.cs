using Atlas.Outbox.Application.Queries.ListOutboxMessages;

namespace Atlas.API.Endpoints.Outbox.ListMessages;

public sealed record ListOutboxMessagesResponse(
    Guid Id,
    Guid ModuleId,
    string ModuleName,
    Guid IdempotencyKey,
    Guid? ParentOutboxMessageId,
    int AttemptNumber,
    string Name,
    string NormalizedName,
    DateTime OccurredOn,
    string Status,
    string Origin,
    string? Error,
    DateTime? ProcessedOn,
    DateTime? FailedAt,
    DateTime? DeadLetteredOn,
    Guid TenantId,
    string? TenantName,
    string? UserEmail,
    string CorrelationId,
    string? ResubmittedByEmail,
    bool HasReplayChild,
    int ExecutionCount,
    IReadOnlyList<OutboxHandlerExecutionDetail> Executions
)
{
    public static ListOutboxMessagesResponse From(OutboxMessageRow row, string? tenantName) =>
        new(
            Id: row.Id,
            ModuleId: row.ModuleId,
            ModuleName: row.ModuleName,
            IdempotencyKey: row.IdempotencyKey,
            ParentOutboxMessageId: row.ParentOutboxMessageId,
            AttemptNumber: row.AttemptNumber,
            Name: row.Name,
            NormalizedName: row.NormalizedName,
            OccurredOn: row.OccurredOn,
            Status: row.Status,
            Origin: row.Origin,
            Error: row.Error,
            ProcessedOn: row.ProcessedOn,
            FailedAt: row.FailedAt,
            DeadLetteredOn: row.DeadLetteredOn,
            TenantId: row.TenantId,
            TenantName: tenantName,
            UserEmail: row.UserEmail,
            CorrelationId: row.CorrelationId,
            ResubmittedByEmail: row.ResubmittedByEmail,
            HasReplayChild: row.HasReplayChild,
            ExecutionCount: row.ExecutionCount,
            Executions: row.Executions
        );
}
