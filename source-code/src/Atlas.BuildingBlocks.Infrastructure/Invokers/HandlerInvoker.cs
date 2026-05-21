using Atlas.BuildingBlocks.Application.Invokers.Decorators;
using Atlas.BuildingBlocks.Application.Invokers.Interfaces;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.Extensions.Logging;

namespace Atlas.BuildingBlocks.Application.Invokers;

/// <summary>
/// Composes and executes the handler decorator pipeline.
/// Navigate to each decorator class to understand what it does.
/// </summary>
public sealed class HandlerInvoker : IHandlerInvoker
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRequestContext _requestContext;

    public HandlerInvoker(ILoggerFactory loggerFactory, IServiceProvider serviceProvider, IRequestContext requestContext)
    {
        _loggerFactory   = loggerFactory;
        _serviceProvider = serviceProvider;
        _requestContext  = requestContext;
    }

    /// <inheritdoc/>
    public Task<Result<TOutput>> InvokeAsync<TInput, TOutput>(
        IHandler<TInput, TOutput> handler,
        TInput input,
        CancellationToken ct)
    {
        // ── Command block ──────────────────────────────────────────────────────
        // Validation and persistence — skipped for queries.
        IHandler<TInput, TOutput> handlerPipeline = handler;
        if (handler is ICommandHandler<TInput, TOutput> cmd)
        {
            handlerPipeline = new ValidationDecorator<TInput, TOutput>(handlerPipeline, _serviceProvider);
            handlerPipeline = new PersistDbDecorator<TInput, TOutput>(handlerPipeline, cmd.UnitOfWork);
        }
        // ───────────────────────────────────────────────────────────────────────

        // ── Observability block — all handlers ─────────────────────────────────
        // Error handling, logging and tracing — wraps both commands and queries.
        var isCommand = handler is ICommandHandler<TInput, TOutput>;
        var layer = isCommand ? "handler" : "query";
        var name  = handler.GetType().Name;

        IResultPipelineStep<TInput, TOutput> pipeline = new OutputTransformDecorator<TInput, TOutput>(handlerPipeline);
        pipeline = new DomainExceptionDecorator<TInput, TOutput>(pipeline);
        pipeline = new LoggingDecorator<TInput, TOutput>(pipeline, _loggerFactory, handler.GetType(), name, layer);
        pipeline = new TelemetryDecorator<TInput, TOutput>(pipeline, name, layer, _requestContext.CorrelationId);
        // ───────────────────────────────────────────────────────────────────────

        return pipeline.ExecuteAsync(input, ct);
    }
}
