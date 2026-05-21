using Atlas.BuildingBlocks.Application.HandlerInvokers.Decorators;
using Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.Extensions.Logging;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers;

/// <summary>
/// Composes and executes the integration event handler decorator pipeline.
/// Internal — instantiated by HandlerInvoker (the public router).
///
/// Pipeline (innermost → outermost):
///   handler (IHandler&lt;TEvent, Unit&gt;)
///     IntegrationIdempotencyDecorator  ← idempotency guard (IIdempotentHandler opt-in)
///       OutputTransformDecorator       ← Unit → Result&lt;Unit&gt; boundary
///         DomainExceptionDecorator     ← domain/validation exceptions → Result.Fail
///           LoggingDecorator           ← structured log before + after
///             TelemetryDecorator       ← OTel span per handler
///
/// The four outer decorators are the same generic classes used by CommandHandlerInvoker
/// and QueryHandlerInvoker — no duplication.
///
/// IIdempotencyContext is populated by OutboxMessageDispatcher before each invocation.
/// CorrelationId is read from IRequestContext (populated by WorkerRequestContext).
/// </summary>
internal sealed class IntegrationEventHandlerInvoker
{
    private readonly ILoggerFactory  _loggerFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRequestContext  _requestContext;

    public IntegrationEventHandlerInvoker(
        ILoggerFactory   loggerFactory,
        IServiceProvider serviceProvider,
        IRequestContext  requestContext)
    {
        _loggerFactory   = loggerFactory;
        _serviceProvider = serviceProvider;
        _requestContext  = requestContext;
    }

    public Task<Result<Unit>> InvokeAsync<TEvent>(
        IHandler<TEvent, Unit> handler,
        TEvent                 input,
        CancellationToken      ct)
    {
        var handlerType = handler.GetType();
        var handlerName = handlerType.Name;

        IHandler<TEvent, Unit> handlerPipeline;
        IResultPipelineStep<TEvent, Unit> resultPipeline;
        // ── Integration-specific inner chain ──────────────────────────────────
        handlerPipeline = new IntegrationIdempotencyDecorator<TEvent>(handler, _serviceProvider);
        resultPipeline = new OutputTransformDecorator<TEvent, Unit>(handlerPipeline);
        resultPipeline = new DomainExceptionDecorator<TEvent, Unit>(resultPipeline);
        resultPipeline = new LoggingDecorator<TEvent, Unit>(resultPipeline, _loggerFactory, handlerType, handlerName, layer: "integration");
        resultPipeline = new TelemetryDecorator<TEvent, Unit>(resultPipeline, handlerName, layer: "integration", _requestContext.CorrelationId);
        // ─────────────────────────────────────────────────────────────────────

        return resultPipeline.ExecuteAsync(input, ct);
    }
}
