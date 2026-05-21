using Atlas.BuildingBlocks.Application.HandlerInvokers.Decorators;
using Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.Extensions.Logging;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers;

/// <summary>
/// Executes a query handler through the observability-only decorator pipeline.
/// Validation and persistence are intentionally excluded for queries.
///
///   TelemetryDecorator
///     LoggingDecorator
///       DomainExceptionDecorator
///         OutputTransformDecorator
///           handler
/// </summary>
internal sealed class QueryHandlerInvoker
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IRequestContext _requestContext;

    public QueryHandlerInvoker(
        ILoggerFactory loggerFactory,
        IRequestContext requestContext)
    {
        _loggerFactory  = loggerFactory;
        _requestContext = requestContext;
    }

    public Task<Result<TOutput>> InvokeAsync<TInput, TOutput>(
        IHandler<TInput, TOutput> handler,
        TInput input,
        CancellationToken ct)
    {
        var name = handler.GetType().Name;

        // ── Observability block ────────────────────────────────────────────
        IResultPipelineStep<TInput, TOutput> pipeline = new OutputTransformDecorator<TInput, TOutput>(handler);
        pipeline = new DomainExceptionDecorator<TInput, TOutput>(pipeline);
        pipeline = new LoggingDecorator<TInput, TOutput>(pipeline, _loggerFactory, handler.GetType(), name, layer: "query");
        pipeline = new TelemetryDecorator<TInput, TOutput>(pipeline, name, layer: "query", _requestContext.CorrelationId);
        // ──────────────────────────────────────────────────────────────────

        return pipeline.ExecuteAsync(input, ct);
    }
}
