using System.Diagnostics;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Dispatching;
using Microsoft.Extensions.Logging;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers;

/// <summary>
/// Decorator that adds structured logging around any dispatcher.
///
/// Reads observability data from <see cref="ITraceContext"/> (populated before each dispatch
/// by ProcessOutboxCommandHandler) — the decorator itself has no knowledge of the concrete
/// message type, keeping the pipeline fully generic.
///
/// Logs:
///   • Entry  — event type, message id, attempt number, correlation id.
///   • Exit   — event type, message id, elapsed milliseconds.
///   • Error  — event type, message id, elapsed milliseconds, exception.
///
/// Sits outside <see cref="TracingDispatcherDecorator{TMessage,TResult}"/> in the pipeline
/// so log entries are always emitted even when no OTel trace context is present.
/// </summary>
internal sealed class LoggingDispatcherDecorator<TMessage, TResult>
    : IDispatcher<TMessage, TResult>
{
    private readonly IDispatcher<TMessage, TResult> _inner;
    private readonly ITraceContext                  _traceContext;
    private readonly ILogger                        _logger;

    public LoggingDispatcherDecorator(
        IDispatcher<TMessage, TResult> inner,
        ITraceContext                  traceContext,
        ILoggerFactory                 loggerFactory)
    {
        _inner        = inner;
        _traceContext = traceContext;
        _logger       = loggerFactory.CreateLogger("LoggingDispatcherDecorator");
    }

    public async Task<TResult> DispatchAsync(TMessage message, CancellationToken ct)
    {
        _logger.LogInformation(
            "Dispatching {EventType} [id: {MessageId}, attempt: {AttemptNumber}, correlation: {CorrelationId}]",
            _traceContext.MessageName,
            _traceContext.MessageId,
            _traceContext.AttemptNumber,
            _traceContext.CorrelationId);

        var sw = Stopwatch.StartNew();
        try
        {
            var result = await _inner.DispatchAsync(message, ct);
            sw.Stop();

            _logger.LogInformation(
                "Dispatched {EventType} [id: {MessageId}] in {ElapsedMs}ms",
                _traceContext.MessageName,
                _traceContext.MessageId,
                sw.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "Failed to dispatch {EventType} [id: {MessageId}] after {ElapsedMs}ms",
                _traceContext.MessageName,
                _traceContext.MessageId,
                sw.ElapsedMilliseconds);
            throw;
        }
    }
}
