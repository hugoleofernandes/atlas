namespace Atlas.Outbox.Application.Queries.ListDeadLetters;

public interface IListDeadLettersReader
{
    Task<IReadOnlyList<DeadLetterSummary>> ReadAsync(CancellationToken ct);
}
