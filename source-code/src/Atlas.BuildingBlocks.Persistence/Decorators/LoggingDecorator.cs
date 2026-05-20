using Atlas.BuildingBlocks.Persistence.DbContexts;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Atlas.BuildingBlocks.Persistence.Decorators;

internal sealed class LoggingDecorator : ISavePipelineStep
{
    private readonly ISavePipelineStep _inner;
    private readonly ILogger<SavePipeline> _logger;
    private readonly string _operationName;

    public LoggingDecorator(ISavePipelineStep inner, ILogger<SavePipeline> logger, string operationName)
    {
        _inner         = inner;
        _logger        = logger;
        _operationName = operationName;
    }

    public async Task ExecuteAsync(DbContextBase db, CancellationToken ct)
    {
        _logger.LogInformation("{Operation} started", _operationName);
        var sw = Stopwatch.StartNew();

        await _inner.ExecuteAsync(db, ct);

        sw.Stop();
        _logger.LogInformation("{Operation} succeeded in {ElapsedMs}ms", _operationName, sw.ElapsedMilliseconds);
    }
}
