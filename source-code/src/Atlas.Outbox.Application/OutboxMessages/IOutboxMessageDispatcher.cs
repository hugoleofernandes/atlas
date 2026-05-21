using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.OutboxMessages;

public interface IOutboxMessageDispatcher
{
    /// <summary>
    /// Resolves all handlers registered for the event type carried by <paramref name="message"/>,
    /// invokes each one through IHandlerInvoker, and returns one
    /// HandlerInvocationResult per handler.
    ///
    /// Dispatcher-level failures (unknown type, deserialization error, no handlers registered)
    /// are thrown as exceptions. Handler-level failures are captured inside each result.
    /// </summary>
    Task<IReadOnlyList<HandlerInvocationResult>> DispatchAsync(OutboxMessage message, CancellationToken ct);
}
