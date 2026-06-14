namespace Atlas.Outbox.Application.Queries.ListOutboxMessages;

public interface IListOutboxMessagesReader
{
    Task<IReadOnlyList<OutboxMessageRow>> ReadAsync(DateTime from, DateTime to, CancellationToken ct);
}
