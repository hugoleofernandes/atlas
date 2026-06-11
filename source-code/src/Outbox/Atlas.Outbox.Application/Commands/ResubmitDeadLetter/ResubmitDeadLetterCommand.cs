namespace Atlas.Outbox.Application.Commands.ResubmitDeadLetter;

public sealed record ResubmitDeadLetterCommand(Guid MessageId);
