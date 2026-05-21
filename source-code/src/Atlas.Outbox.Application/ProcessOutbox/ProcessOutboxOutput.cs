namespace Atlas.Outbox.Application.ProcessOutbox;

public record ProcessOutboxOutput(int Processed, int Failed, int DeadLettered);
