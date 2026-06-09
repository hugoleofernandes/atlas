namespace Atlas.SharedKernel.Application;

/// <summary>
/// Marker contract for any component that processes a message and returns a result.
///
/// Implemented by concrete dispatchers (e.g. <c>IOutboxMessageDispatcher</c>) and by every
/// decorator in a dispatcher pipeline — making it possible to chain decorators without
/// coupling them to a specific dispatcher type.
/// </summary>
public interface IDispatcher<TMessage, TResult>
{
    Task<TResult> DispatchAsync(TMessage message, CancellationToken ct);
}
