namespace Atlas.Outbox.Contracts;

public interface IGetPendingMessagesReader
{
    Task<IReadOnlyList<OutboxMessageDto>> ReadAsync(
        int batchSize,
        TimeSpan lockDuration,
        CancellationToken ct);
}
