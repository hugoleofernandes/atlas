using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.OutboxMessages;

public interface IOutboxMessageDispatcher
{
    Task DispatchAsync(OutboxMessage message, CancellationToken ct);
}
