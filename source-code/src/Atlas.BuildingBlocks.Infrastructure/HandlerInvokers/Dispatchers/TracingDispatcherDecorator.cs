using System.Diagnostics;
using Atlas.BuildingBlocks.Infrastructure.Observability;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Dispatching;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers;

/// <summary>
/// Decorator that handles distributed trace continuation for any dispatcher.
///
/// Reads the W3C <c>TraceParent</c> from <see cref="ITraceContext"/> (populated before
/// each dispatch by ProcessOutboxCommandHandler) and opens an <c>outbox.process_message</c>
/// span as a child of the original API span before delegating to the inner dispatcher.
/// All handler and adapter spans created by <c>TelemetryDecorator</c> during that dispatch
/// automatically nest under it — producing a single end-to-end trace in Grafana Tempo.
///
/// When <c>TraceParent</c> is null (no OTel context at publish time — seeding, tests, CLI)
/// the inner dispatcher runs without a parent span: graceful degradation, no errors.
///
/// Generic over <typeparamref name="TMessage"/> and <typeparamref name="TResult"/> so it
/// works with any dispatcher type without coupling to a specific message class.
/// </summary>
internal sealed class TracingDispatcherDecorator<TMessage, TResult>
    : IDispatcher<TMessage, TResult>
{
    private readonly IDispatcher<TMessage, TResult> _inner;
    private readonly ITraceContext                  _traceContext;

    public TracingDispatcherDecorator(
        IDispatcher<TMessage, TResult> inner,
        ITraceContext                  traceContext)
    {
        _inner        = inner;
        _traceContext = traceContext;
    }

    public async Task<TResult> DispatchAsync(TMessage message, CancellationToken ct)
    {
        if (_traceContext.TraceParent is null ||
            !ActivityContext.TryParse(_traceContext.TraceParent, null, isRemote: true, out var parentContext))
        {
            // No trace context stored — delegate directly, no span opened.
            return await _inner.DispatchAsync(message, ct);
        }

        using var activity = AtlasActivitySource.Source.StartActivity(
            "outbox.process_message",
            ActivityKind.Consumer,
            parentContext);

        activity?.SetTag("messaging.message_id",     _traceContext.MessageId.ToString());
        activity?.SetTag("messaging.event_type",     _traceContext.MessageName);
        activity?.SetTag("messaging.attempt_number", _traceContext.AttemptNumber);
        activity?.SetTag("correlation_id",           _traceContext.CorrelationId);

        try
        {
            return await _inner.DispatchAsync(message, ct);
        }
        catch
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            throw;
        }
    }
}
