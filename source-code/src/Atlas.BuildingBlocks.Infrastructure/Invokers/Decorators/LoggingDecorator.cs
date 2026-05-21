using Atlas.BuildingBlocks.Application.Invokers.Interfaces;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Errors;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Atlas.BuildingBlocks.Application.Invokers.Decorators;

/// <summary>
/// Structured logging for the handler pipeline.
///
/// - Before execution : logs "{Name} started"
/// - On Result.Ok     : logs "{Name} succeeded in {ms}ms" (Info)
/// - On Result.Fail   : logs "{Name} [validation] failed in {ms}ms — {code}" (Warning)
/// - On exception     : logs "{Name} failed unexpectedly in {ms}ms" (Error) and re-throws
///
/// Unexpected exceptions (not caught by <see cref="DomainExceptionDecorator{TInput,TOutput}"/>)
/// are logged as Error and re-thrown so they continue to propagate up the pipeline.
/// </summary>
internal sealed class LoggingDecorator<TInput, TOutput> : IResultPipelineStep<TInput, TOutput>
{
    private readonly IResultPipelineStep<TInput, TOutput> _inner;
    private readonly ILogger _logger;
    private readonly string _name;
    private readonly string _layer;

    public LoggingDecorator(
        IResultPipelineStep<TInput, TOutput> inner,
        ILoggerFactory loggerFactory,
        Type handlerType,
        string name,
        string layer)
    {
        _inner  = inner;
        _logger = loggerFactory.CreateLogger(handlerType);
        _name   = name;
        _layer  = layer;
    }

    public async Task<Result<TOutput>> ExecuteAsync(TInput input, CancellationToken ct)
    {
        using var _ = _logger.BeginScope(
            new Dictionary<string, object?> { [$"{_layer}Name"] = _name });

        _logger.LogInformation("{Name} started", _name);
        var sw = Stopwatch.StartNew();

        try
        {
            var result = await _inner.ExecuteAsync(input, ct);
            sw.Stop();

            if (result.IsSuccess)
            {
                _logger.LogInformation("{Name} succeeded in {ElapsedMs}ms",
                    _name, sw.ElapsedMilliseconds);
            }
            else
            {
                var error = result.ErrorDefinition!;

                if (error.Category == ErrorCategory.Validation)
                    _logger.LogWarning("{Name} validation failed in {ElapsedMs}ms — {ErrorCode}",
                        _name, sw.ElapsedMilliseconds, error.Code);
                else
                    _logger.LogWarning("{Name} failed in {ElapsedMs}ms — {ErrorCode}",
                        _name, sw.ElapsedMilliseconds, error.Code);
            }

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "{Name} failed unexpectedly in {ElapsedMs}ms",
                _name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
