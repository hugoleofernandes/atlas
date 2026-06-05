namespace Atlas.Outbox.Application.Commands.UpdateOutboxMessageStatus;

public sealed record UpdateOutboxMessageStatusOutput(
    OutboxMessageFinalStatus Status,
    int ExecutionCount,
    int FailureCount
);
