using Atlas.BuildingBlocks.Persistence.DbContexts;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Atlas.BuildingBlocks.Persistence.Decorators;

internal sealed class LoggingDecorator : ISavePipelineStep
{
    private readonly ISavePipelineStep _inner;
    private readonly ILogger<SavePipeline> _logger;

    public LoggingDecorator(ISavePipelineStep inner, ILogger<SavePipeline> logger)
    {
        _inner  = inner;
        _logger = logger;
    }

    public async Task ExecuteAsync(DbContextBase db, CancellationToken ct)
    {
        _logger.LogInformation("SavePipeline started");
        var sw = Stopwatch.StartNew();

        await _inner.ExecuteAsync(db, ct);

        sw.Stop();
        _logger.LogInformation("SavePipeline succeeded in {ElapsedMs}ms", sw.ElapsedMilliseconds);
    }
}
