using Atlas.BuildingBlocks.Infrastructure.Observability;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Atlas.BuildingBlocks.Infrastructure.Workflows;

public abstract class QueryHandlerBase<TQuery, TOutput>
{
    private readonly ILogger _logger;

    protected QueryHandlerBase(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task<TOutput> ExecuteAsync(TQuery query, CancellationToken ct)
    {
        var queryName = GetType().Name;
        var sw = Stopwatch.StartNew();

        using var activity = AtlasActivitySource.Source
            .StartActivity($"Query {queryName}", ActivityKind.Internal);
        activity?.SetTag("atlas.query", queryName);
        activity?.SetTag("atlas.layer", "query");

        using (_logger.BeginScope(new Dictionary<string, object?> { ["QueryName"] = queryName }))
        {
            _logger.LogInformation("QueryHandler {Query} started", queryName);

            var result = await HandleAsync(query, ct);

            _logger.LogInformation("QueryHandler {Query} succeeded in {ElapsedMs}ms",
                queryName, sw.ElapsedMilliseconds);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
    }

    protected abstract Task<TOutput> HandleAsync(TQuery query, CancellationToken ct);
}
