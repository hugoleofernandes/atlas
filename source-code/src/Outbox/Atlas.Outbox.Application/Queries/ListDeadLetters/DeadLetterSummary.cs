namespace Atlas.Outbox.Application.Queries.ListDeadLetters;

public sealed record DeadLetterSummary(
    Guid     Id,
    string   Name,
    string   Module,
    int      AttemptNumber,
    DateTime DeadLetteredOn,
    string?  Error,
    bool     WasResubmitted)
{
    public bool HasReplayChild => WasResubmitted;
}
