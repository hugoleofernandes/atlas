using Atlas.BuildingBlocks.Infrastructure.Observability;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Atlas.BuildingBlocks.Infrastructure.Workflows;

public abstract class CommandHandlerBase<TCommand, TOutput>
{
    private readonly ILogger _logger;

    protected CommandHandlerBase(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task<TOutput> ExecuteAsync(TCommand cmd, CancellationToken ct)
    {
        var handlerName = GetType().Name;
        var sw = Stopwatch.StartNew();

        using var activity = AtlasActivitySource.Source
            .StartActivity($"Handler {handlerName}", ActivityKind.Internal);
        activity?.SetTag("atlas.handler", handlerName);
        activity?.SetTag("atlas.layer", "handler");

        using (_logger.BeginScope(new Dictionary<string, object?> { ["HandlerName"] = handlerName }))
        {
            _logger.LogInformation("CommandHandler {Handler} started", handlerName);

            var result = await HandleAsync(cmd, ct);

            _logger.LogInformation("CommandHandler {Handler} succeeded in {ElapsedMs}ms",
                handlerName, sw.ElapsedMilliseconds);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
    }

    protected abstract Task<TOutput> HandleAsync(TCommand cmd, CancellationToken ct);
}
