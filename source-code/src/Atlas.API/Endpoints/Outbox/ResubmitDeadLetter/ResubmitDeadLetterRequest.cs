namespace Atlas.API.Endpoints.Outbox.ResubmitDeadLetter;

public sealed class ResubmitDeadLetterRequest
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
}
