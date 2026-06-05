using Atlas.Outbox.Contracts.Queries.ListPendingMessages;

namespace Atlas.Outbox.Application.Queries.GetPendingMessages;

public sealed class GetPendingMessagesQueryHandler(IListPendingMessagesReader reader) : IListPendingMessagesQueryHandler
{
    public Task<IReadOnlyList<ListPendingMessagesDto>> ExecuteAsync(
        ListPendingMessagesQuery query,
        CancellationToken ct
    ) => reader.ReadAsync(query.BatchSize, query.LockDuration, ct);
}
