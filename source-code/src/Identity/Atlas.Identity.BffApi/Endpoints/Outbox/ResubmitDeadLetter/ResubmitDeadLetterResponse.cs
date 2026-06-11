using Atlas.Outbox.Application.Commands.ResubmitDeadLetter;

namespace Atlas.Identity.BffApi.Endpoints.Outbox.ResubmitDeadLetter;

public sealed record ResubmitDeadLetterResponse(Guid NewMessageId)
{
    public static ResubmitDeadLetterResponse From(ResubmitDeadLetterOutput output)
        => new(output.NewMessageId);
}
