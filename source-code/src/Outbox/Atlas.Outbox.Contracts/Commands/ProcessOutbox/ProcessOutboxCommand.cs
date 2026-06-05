namespace Atlas.Outbox.Contracts.Commands.ProcessOutbox;

public record ProcessOutboxCommand(int BatchSize, int MaxRetries, TimeSpan LockDuration);
