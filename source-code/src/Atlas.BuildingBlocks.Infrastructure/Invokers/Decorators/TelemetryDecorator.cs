using Atlas.BuildingBlocks.Application.Invokers.Interfaces;
using Atlas.BuildingBlocks.Infrastructure.Observability;
using Atlas.SharedKernel.Application.Commands;
using System.Diagnostics;

namespace Atlas.BuildingBlocks.Application.Invokers.Decorators;

/// <summary>
/// OpenTelemetry decorator. Opens an activity span that covers the full pipeline execution.
///
/// - On Result.Ok   : sets span status to Ok
/// - On Result.Fail : sets span status to Error with error code and category tags
/// - On exception   : sets span status to Error and re-throws
/// </summary>
internal sealed class TelemetryDecorator<TInput, TOutput> : IResultPipelineStep<TInput, TOutput>
{
    private readonly IResultPipelineStep<TInput, TOutput> _inner;
    private readonly string _name;
    private readonly string _layer;
    private readonly string? _correlationId;

    public TelemetryDecorator(IResultPipelineStep<TInput, TOutput> inner, string name, string layer, string? correlationId = null)
    {
        _inner         = inner;
        _name          = name;
        _layer         = layer;
        _correlationId = correlationId;
    }

    public async Task<Result<TOutput>> ExecuteAsync(TInput input, CancellationToken ct)
    {
        var spanName = _layer == "handler" ? $"Handler {_name}" : $"Query {_name}";

        using var activity = AtlasActivitySource.Source.StartActivity(spanName, ActivityKind.Internal);
        activity?.SetTag("atlas.layer",        _layer);
        activity?.SetTag($"atlas.{_layer}",    _name);
        activity?.SetTag("correlation_id",     _correlationId);

        try
        {
            var result = await _inner.ExecuteAsync(input, ct);

            if (result.IsSuccess)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                var error = result.ErrorDefinition!;
                activity?.SetStatus(ActivityStatusCode.Error, error.Code);
                activity?.SetTag("atlas.error.code",     error.Code);
                activity?.SetTag("atlas.error.category", error.Category.ToString());
            }

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
