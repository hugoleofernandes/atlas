namespace Atlas.Outbox.Contracts.Queries.ListPendingMessages;

public interface IListPendingMessagesReader
{
    Task<IReadOnlyList<ListPendingMessagesDto>> ReadAsync(int batchSize, TimeSpan lockDuration, CancellationToken ct);
}
