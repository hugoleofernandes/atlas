namespace Atlas.Outbox.Application.Queries.ListDeadLetters;

public sealed class ListDeadLettersQueryHandler(IListDeadLettersReader reader)
    : IListDeadLettersQueryHandler
{
    public Task<IReadOnlyList<DeadLetterSummary>> ExecuteAsync(
        ListDeadLettersQuery query,
        CancellationToken ct)
        => reader.ReadAsync(ct);
}
