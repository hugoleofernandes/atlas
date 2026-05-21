using Atlas.BuildingBlocks.Application.HandlerInvokers.Decorators;
using Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.Extensions.Logging;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers;

/// <summary>
/// Executes a command handler through the full decorator pipeline:
///
///   TelemetryDecorator
///     LoggingDecorator
///       DomainExceptionDecorator
///         OutputTransformDecorator
///           ValidationDecorator   ← validates input via FluentValidation
///             PersistDbDecorator  ← calls UnitOfWork.SaveChangesAsync after handler
///               handler
/// </summary>
internal sealed class CommandHandlerInvoker
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRequestContext _requestContext;

    public CommandHandlerInvoker(
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider,
        IRequestContext requestContext)
    {
        _loggerFactory   = loggerFactory;
        _serviceProvider = serviceProvider;
        _requestContext  = requestContext;
    }

    public Task<Result<TOutput>> InvokeAsync<TInput, TOutput>(
        ICommandHandler<TInput, TOutput> handler,
        TInput input,
        CancellationToken ct)
    {
        var name = handler.GetType().Name;

        // ── Command block ──────────────────────────────────────────────────
        IHandler<TInput, TOutput> handlerPipeline = handler;
        handlerPipeline = new ValidationDecorator<TInput, TOutput>(handlerPipeline, _serviceProvider);
        handlerPipeline = new PersistDbDecorator<TInput, TOutput>(handlerPipeline, handler.UnitOfWork);
        // ──────────────────────────────────────────────────────────────────

        // ── Observability block ────────────────────────────────────────────
        IResultPipelineStep<TInput, TOutput> pipeline = new OutputTransformDecorator<TInput, TOutput>(handlerPipeline);
        pipeline = new DomainExceptionDecorator<TInput, TOutput>(pipeline);
        pipeline = new LoggingDecorator<TInput, TOutput>(pipeline, _loggerFactory, handler.GetType(), name, layer: "handler");
        pipeline = new TelemetryDecorator<TInput, TOutput>(pipeline, name, layer: "handler", _requestContext.CorrelationId);
        // ──────────────────────────────────────────────────────────────────

        return pipeline.ExecuteAsync(input, ct);
    }
}
