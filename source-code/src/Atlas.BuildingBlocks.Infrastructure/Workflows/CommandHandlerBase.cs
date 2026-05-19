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

        using (_logger.BeginScope(new Dictionary<string, object?> { ["HandlerName"] = handlerName }))
        {
            _logger.LogInformation("CommandHandler {Handler} started", handlerName);

            var result = await HandleAsync(cmd, ct);

            _logger.LogInformation("CommandHandler {Handler} succeeded in {ElapsedMs}ms",
                handlerName, sw.ElapsedMilliseconds);

            return result;
        }
    }

    protected abstract Task<TOutput> HandleAsync(TCommand cmd, CancellationToken ct);
}
