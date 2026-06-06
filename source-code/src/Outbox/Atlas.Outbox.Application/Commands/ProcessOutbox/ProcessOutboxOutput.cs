namespace Atlas.Outbox.Application.Commands.ProcessOutbox;

public record ProcessOutboxOutput(int Processed, int Failed, int DeadLettered);
