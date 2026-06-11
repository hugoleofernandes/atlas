namespace Atlas.Outbox.Application.Commands.ProcessOutbox;

public record ProcessOutboxCommand(int BatchSize, int MaxRetries, TimeSpan LockDuration, string Module);
