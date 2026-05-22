namespace Atlas.SharedKernel.Application.Dispatching;

/// <summary>
/// Holds the observability data for the message currently being dispatched.
///
/// Populated by ProcessOutboxCommandHandler before each dispatch — the same pattern
/// used by IRequestContext and IIdempotencyContext.
///
/// Consumed by dispatcher decorators (tracing, logging) so they remain generic
/// and never need to read directly from a specific message type.
/// </summary>
public interface ITraceContext
{
    /// <summary>W3C traceparent stored at publish time. Null when no OTel context was active.</summary>
    string? TraceParent   { get; }

    /// <summary>Human-readable event name (e.g. "user.created_from_invitation").</summary>
    string  MessageName   { get; }

    /// <summary>Unique identifier of the outbox message row.</summary>
    Guid    MessageId     { get; }

    /// <summary>1-based attempt number within the retry chain.</summary>
    int     AttemptNumber { get; }

    /// <summary>Correlation id propagated across the entire request chain.</summary>
    string  CorrelationId { get; }
}
