using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.OutboxWorker.Dispatching;

public interface IOutboxMessageDispatcher
{
    Task DispatchAsync(OutboxMessage message, CancellationToken ct);
}
