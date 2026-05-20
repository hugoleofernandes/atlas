using Atlas.BuildingBlocks.Persistence.DbContexts;
using System.Diagnostics;

namespace Atlas.BuildingBlocks.Persistence.Decorators;

internal sealed class TelemetryDecorator : ISavePipelineStep
{
    private static readonly ActivitySource _source = new("Atlas", "1.0.0");

    private readonly ISavePipelineStep _inner;
    private readonly string _operationName;

    public TelemetryDecorator(ISavePipelineStep inner, string operationName)
    {
        _inner         = inner;
        _operationName = operationName;
    }

    public async Task ExecuteAsync(DbContextBase db, CancellationToken ct)
    {
        using var activity = _source.StartActivity(_operationName, ActivityKind.Internal);
        activity?.SetTag("atlas.layer", "persistence");

        await _inner.ExecuteAsync(db, ct);

        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
