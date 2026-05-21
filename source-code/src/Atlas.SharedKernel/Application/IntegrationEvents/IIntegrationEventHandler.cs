using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.SharedKernel.Application.IntegrationEvents;

/// <summary>
/// Non-generic marker — lets HandlerInvoker detect integration event handlers
/// without reflection, even when the handler is held as IHandler&lt;TEvent, Unit&gt;.
/// </summary>
public interface IIntegrationEventHandler { }

/// <summary>
/// Contract for integration event handlers.
///
/// Extends IHandler&lt;TEvent, Unit&gt; so integration event handlers are first-class
/// members of the Result&lt;TOutput&gt; family alongside command and query handlers.
/// HandlerInvoker routes them to IntegrationEventHandlerInvoker automatically.
///
/// Implement HandleAsync — ExecuteAsync is provided as a default interface method
/// that calls HandleAsync and returns Unit.Value. No changes needed in existing handlers.
/// </summary>
public interface IIntegrationEventHandler<TEvent>
    : IHandler<TEvent, Unit>, IIntegrationEventHandler
{
    /// <summary>
    /// Handle the integration event.
    /// Implement this method — ExecuteAsync delegates here automatically.
    /// </summary>
    Task HandleAsync(TEvent @event, CancellationToken ct);

    /// <summary>
    /// Bridges IHandler&lt;TEvent, Unit&gt;.ExecuteAsync to HandleAsync.
    /// Provided as a default implementation — handlers do not override this.
    /// </summary>
    async Task<Unit> IHandler<TEvent, Unit>.ExecuteAsync(TEvent input, CancellationToken ct)
    {
        await HandleAsync(input, ct);
        return Unit.Value;
    }
}
