namespace Atlas.SharedKernel.Application.OutboxMessages;

public interface IOutboxMessageRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken ct);

    Task AddRangeAsync(IEnumerable<OutboxMessage> messages, CancellationToken ct);
}