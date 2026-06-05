using Atlas.Outbox.Contracts;

namespace Atlas.Outbox.Application.Queries.GetPendingMessages;

public sealed class GetPendingMessagesQueryHandler(IGetPendingMessagesReader reader)
    : IGetPendingMessagesQueryHandler
{
    public Task<IReadOnlyList<OutboxMessageDto>> ExecuteAsync(
        GetPendingMessagesQuery query,
        CancellationToken ct)
        => reader.ReadAsync(query.BatchSize, query.LockDuration, ct);
}
