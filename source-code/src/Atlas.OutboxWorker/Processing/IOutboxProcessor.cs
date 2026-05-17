namespace Atlas.OutboxWorker.Processing;

public interface IOutboxProcessor
{
    Task ProcessBatchAsync(CancellationToken ct);
}
