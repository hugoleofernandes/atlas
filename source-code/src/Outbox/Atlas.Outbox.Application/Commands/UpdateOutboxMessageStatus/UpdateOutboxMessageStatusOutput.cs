using Atlas.Outbox.Domain.Processing;

namespace Atlas.Outbox.Application.Commands.UpdateOutboxMessageStatus;

public sealed record UpdateOutboxMessageStatusOutput(
    OutboxMessageFinalStatus Status,
    int ExecutionCount,
    int FailureCount
);
