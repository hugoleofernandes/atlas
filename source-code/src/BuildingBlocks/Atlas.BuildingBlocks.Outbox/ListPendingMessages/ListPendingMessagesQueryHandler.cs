namespace Atlas.BuildingBlocks.Outbox.ListPendingMessages;

public sealed class ListPendingMessagesQueryHandler(IListPendingMessagesReader reader)
    : IListPendingMessagesQueryHandler
{
    public Task<IReadOnlyList<ListPendingMessagesDto>> ExecuteAsync(
        ListPendingMessagesQuery query,
        CancellationToken ct
    ) => reader.ReadAsync(query.BatchSize, query.LockDuration, ct);
}
