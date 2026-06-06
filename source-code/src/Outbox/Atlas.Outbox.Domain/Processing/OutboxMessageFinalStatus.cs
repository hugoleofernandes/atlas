namespace Atlas.Outbox.Domain.Processing;

public enum OutboxMessageFinalStatus
{
    Processed = 1,
    RetryScheduled = 2,
    DeadLettered = 3,
}
