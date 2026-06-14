namespace Atlas.Outbox.Application.Queries.ListDeadLetters;

public sealed class ListDeadLettersQueryHandler(IListDeadLettersReader reader)
    : IIdentityListDeadLettersQueryHandler,
      IStaffListDeadLettersQueryHandler
{
    public Task<IReadOnlyList<DeadLetterSummary>> ExecuteAsync(
        ListDeadLettersQuery query,
        CancellationToken ct)
        => reader.ReadAsync(ct);
}
