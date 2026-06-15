using Atlas.Outbox.Application.Commands.ResubmitDeadLetter;

namespace Atlas.API.Endpoints.Outbox.ResubmitDeadLetter;

public sealed record ResubmitDeadLetterResponse(
    Guid ModuleId,
    string ModuleName,
    Guid NewMessageId)
{
    public static ResubmitDeadLetterResponse From(
        ResubmitDeadLetterOutput output,
        Guid moduleId,
        string moduleName)
        => new(moduleId, moduleName, output.NewMessageId);
}
