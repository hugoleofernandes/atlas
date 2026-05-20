using Atlas.BuildingBlocks.Infrastructure.Workflows.Decorators;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.Extensions.Logging;

namespace Atlas.BuildingBlocks.Infrastructure.Workflows;

/// <summary>
/// Composes and executes the handler decorator pipeline.
/// Navigate to each decorator class to understand what it does.
/// </summary>
public sealed class HandlerInvoker : IHandlerInvoker
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _serviceProvider;

    public HandlerInvoker(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    {
        _loggerFactory   = loggerFactory;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public Task<Result<TOutput>> InvokeAsync<TInput, TOutput>(
        IHandler<TInput, TOutput> handler,
        TInput input,
        CancellationToken ct)
    {
        // ── INNER PIPELINE — commands only ────────────────────────────────
        IHandler<TInput, TOutput> inner = handler;
        var layer = "query";

        if (handler is ICommandHandler<TInput, TOutput> cmd)
        {
            inner = new ValidationDecorator<TInput, TOutput>(inner, _serviceProvider);
            inner = new PersistDbDecorator<TInput, TOutput>(inner, cmd.UnitOfWork);
            layer = "handler";
        }

        // ── OUTER PIPELINE — all handlers ─────────────────────────────────
        IResultHandler<TInput, TOutput> pipeline = new OutputTransformDecorator<TInput, TOutput>(inner);
        pipeline = new DomainExceptionDecorator<TInput, TOutput>(pipeline);

        var logger = _loggerFactory.CreateLogger(handler.GetType());
        var name = handler.GetType().Name;
        pipeline = new LoggingDecorator<TInput, TOutput>(pipeline, logger, name, layer);

        pipeline = new TelemetryDecorator<TInput, TOutput>(pipeline, name, layer);

        return pipeline.ExecuteAsync(input, ct);
    }
}
