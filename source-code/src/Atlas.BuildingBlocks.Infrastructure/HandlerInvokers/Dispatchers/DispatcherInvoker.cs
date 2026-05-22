using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Dispatching;
using Microsoft.Extensions.Logging;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers;

/// <summary>
/// Wraps any dispatcher in the standard observability pipeline and delegates.
///
/// The pipeline applied to every dispatch:
///
///   LoggingDispatcherDecorator   ← structured log per message (entry / exit / error)
///     TracingDispatcherDecorator ← restores W3C traceparent from ITraceContext
///       dispatcher (core)
///
/// Decorators read from <see cref="ITraceContext"/> — set by ProcessOutboxCommandHandler
/// before each dispatch — so they remain fully generic and have zero knowledge of
/// the concrete message type.
///
/// Analogous to <see cref="HandlerInvoker"/> for application handlers:
/// generic at the method level so callers enjoy full type inference and a single
/// registered instance handles every dispatcher type.
/// </summary>
public sealed class DispatcherInvoker : IDispatcherInvoker
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ITraceContext  _traceContext;

    public DispatcherInvoker(ILoggerFactory loggerFactory, ITraceContext traceContext)
    {
        _loggerFactory = loggerFactory;
        _traceContext  = traceContext;
    }

    public Task<TResult> InvokeAsync<TMessage, TResult>(
        IDispatcher<TMessage, TResult> dispatcher,
        TMessage                       message,
        CancellationToken              ct)
    {
        IDispatcher<TMessage, TResult> pipeline = dispatcher;
        pipeline = new LoggingDispatcherDecorator<TMessage, TResult>(pipeline, _traceContext, _loggerFactory);
        pipeline = new TracingDispatcherDecorator<TMessage, TResult>(pipeline, _traceContext);
        return pipeline.DispatchAsync(message, ct);
    }
}
