using Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Application.Logging;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers.Decorators;

/// <summary>
/// Structured logging for the handler pipeline.
///
/// Log strategy — three layers:
///
///   1. Scope (always, no PII risk)
///      Adds InputType and OutputType as structured fields to every log line emitted
///      within the handler execution — including logs from repositories and domain services.
///      Grafana Loki can filter by these fields: {InputType="CreateTenantCommand"}.
///
///   2. Summary at Information level (always, opt-in for business context)
///      If the input implements <see cref="ILogSummary"/>, logs ToLogSummary() — a
///      developer-defined, PII-safe string (identifiers only, no personal data).
///      Without ILogSummary, logs only the input type name.
///
///   3. Full payload at Debug level (off by default, on when investigating)
///      Serializes the entire input object to JSON. Controlled by the log level
///      configuration — set "Default": "Debug" in appsettings to enable.
///      Never enabled in production by default.
///
/// - On Result.Ok   : logs "{Name} succeeded in {ms}ms" (Info)
/// - On Result.Fail : logs "{Name} failed in {ms}ms — {ErrorCode}" (Warning)
/// - On exception   : logs "{Name} failed unexpectedly in {ms}ms" (Error) and re-throws
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
        // ── Layer 1: scope — enriches ALL log lines inside the handler ────────
        using var _ = _logger.BeginScope(new Dictionary<string, object?>
        {
            [$"{_layer}Name"] = _name,
            ["InputType"]     = typeof(TInput).Name,
            ["OutputType"]    = typeof(TOutput).Name,
        });

        // ── Layer 2: summary at Information — PII-safe, always visible ────────
        var summary = input is ILogSummary s ? s.ToLogSummary() : typeof(TInput).Name;
        _logger.LogInformation("{Name} started — {Summary}", _name, summary);

        // ── Layer 3: full payload at Debug — off in production by default ─────
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("{Name} input {@Input}", _name, input);

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
