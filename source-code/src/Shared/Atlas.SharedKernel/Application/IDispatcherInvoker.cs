namespace Atlas.SharedKernel.Application;

/// <summary>
/// Executes any <see cref="IDispatcher{TMessage,TResult}"/> through an explicit decorator pipeline.
///
/// The invoker owns the pipeline composition — the caller supplies the core dispatcher
/// and the invoker wraps it in every cross-cutting concern before delegating.
///
/// Analogous to <see cref="Handlers.IHandlerInvoker"/> for application handlers:
/// generic at the <em>method</em> level, not the interface level — so a single registered
/// instance handles any dispatcher type, and callers benefit from full type inference.
///
/// Concrete implementations live in the BuildingBlocks infrastructure layer and route
/// each <typeparamref name="TMessage"/> to the appropriate decorator pipeline.
/// </summary>
public interface IDispatcherInvoker
{
    Task<TResult> InvokeAsync<TMessage, TResult>(
        IDispatcher<TMessage, TResult> dispatcher,
        TMessage                       message,
        CancellationToken              ct);
}
