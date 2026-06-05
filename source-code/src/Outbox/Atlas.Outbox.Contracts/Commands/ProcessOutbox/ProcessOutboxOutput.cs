namespace Atlas.Outbox.Contracts.Commands.ProcessOutbox;

public record ProcessOutboxOutput(int Processed, int Failed, int DeadLettered);
