namespace Atlas.Outbox.Application.Commands.UpdateOutboxMessageStatus;

public enum OutboxMessageFinalStatus
{
    Processed = 1,
    RetryScheduled = 2,
    DeadLettered = 3,
}
