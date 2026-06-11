namespace Atlas.Outbox.Application.Commands.ResubmitDeadLetter;

public sealed record ResubmitDeadLetterOutput(Guid NewMessageId);
