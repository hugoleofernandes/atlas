namespace Atlas.SharedKernel.Application.OutboxMessages;

public interface IOutboxWorkerRepository
{
    Task<IReadOnlyList<OutboxMessage>> GetPendingBatchAsync(
        int batchSize,
        TimeSpan lockDuration,
        CancellationToken ct);

    /// <summary>
    /// Persists the next-attempt row created by <see cref="OutboxMessage.CreateRetryAttempt"/>.
    /// Must be called within the same Unit of Work as the parent message update so that
    /// closing the parent and inserting the child are committed atomically.
    /// </summary>
    Task AddRetryAsync(OutboxMessage message, CancellationToken ct);

    /// <summary>
    /// Persists the execution records for a single attempt.
    /// Must be called within the same Unit of Work as the parent message update.
    /// </summary>
    Task AddExecutionsAsync(IReadOnlyList<OutboxHandlerExecution> executions, CancellationToken ct);
}
