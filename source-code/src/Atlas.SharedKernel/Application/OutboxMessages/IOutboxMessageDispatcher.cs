namespace Atlas.SharedKernel.Application.OutboxMessages;

/// <summary>
/// Resolves all handlers registered for the event type carried by <paramref name="message"/>,
/// invokes each one through IHandlerInvoker, and returns one
/// HandlerInvocationResult per handler.
///
/// Dispatcher-level failures (unknown type, deserialization error, no handlers registered)
/// are thrown as exceptions. Handler-level failures are captured inside each result.
///
/// Implements <see cref="Application.IDispatcher{TMessage,TResult}"/> so it can be wrapped
/// transparently by any dispatcher decorator in the BuildingBlocks pipeline.
/// </summary>
public interface IOutboxMessageDispatcher
    : Application.IDispatcher<OutboxMessage, IReadOnlyList<HandlerInvocationResult>>
{
}
