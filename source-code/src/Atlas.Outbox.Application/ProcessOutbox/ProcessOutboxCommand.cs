namespace Atlas.Outbox.Application.ProcessOutbox;

public record ProcessOutboxCommand(int BatchSize, int MaxRetries, TimeSpan LockDuration);
