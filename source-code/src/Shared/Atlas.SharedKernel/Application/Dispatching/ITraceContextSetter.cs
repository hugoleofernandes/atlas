namespace Atlas.SharedKernel.Application.Dispatching;

/// <summary>
/// Allows ProcessOutboxCommandHandler to populate <see cref="ITraceContext"/>
/// before each dispatch. Implemented by the same class as <see cref="ITraceContext"/>
/// so both interfaces resolve to the same scoped instance.
/// </summary>
public interface ITraceContextSetter
{
    void Set(string? traceParent, string messageName, Guid messageId, int attemptNumber, string correlationId);
}
