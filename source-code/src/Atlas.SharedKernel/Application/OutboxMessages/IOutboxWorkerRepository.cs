namespace Atlas.SharedKernel.Application.OutboxMessages;

public interface IOutboxWorkerRepository
{
    Task<IReadOnlyList<OutboxMessage>> GetPendingBatchAsync(
        int batchSize,
        TimeSpan lockDuration,
        CancellationToken ct);
}
