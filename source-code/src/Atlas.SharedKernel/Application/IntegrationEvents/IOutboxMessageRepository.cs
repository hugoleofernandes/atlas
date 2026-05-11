namespace Atlas.SharedKernel.Application.IntegrationEvents;

public interface IOutboxMessageRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken ct);

    Task AddRangeAsync(IEnumerable<OutboxMessage> messages, CancellationToken ct);

    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(CancellationToken ct);
}