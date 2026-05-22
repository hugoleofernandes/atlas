using Atlas.SharedKernel.Application.Dispatching;

namespace Atlas.Outbox.Infrastructure;

/// <summary>
/// Scoped context that holds observability data for the message currently being dispatched.
/// ProcessOutboxCommandHandler sets it before each dispatch so dispatcher decorators
/// (tracing, logging) can read it without depending on a specific message type.
/// </summary>
internal sealed class TraceContext : ITraceContext, ITraceContextSetter
{
    private string? _traceParent;
    private string  _messageName   = string.Empty;
    private Guid    _messageId;
    private int     _attemptNumber;
    private string  _correlationId = string.Empty;

    public string? TraceParent   => _traceParent;
    public string  MessageName   => _messageName;
    public Guid    MessageId     => _messageId;
    public int     AttemptNumber => _attemptNumber;
    public string  CorrelationId => _correlationId;

    public void Set(string? traceParent, string messageName, Guid messageId, int attemptNumber, string correlationId)
    {
        _traceParent   = traceParent;
        _messageName   = messageName;
        _messageId     = messageId;
        _attemptNumber = attemptNumber;
        _correlationId = correlationId;
    }
}
