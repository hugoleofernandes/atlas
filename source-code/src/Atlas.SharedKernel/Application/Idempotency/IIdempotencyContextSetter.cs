namespace Atlas.SharedKernel.Application.Idempotency;

/// <summary>
/// Allows the OutboxMessageDispatcher to populate IIdempotencyContext
/// before each handler invocation. Implemented by the same class as IIdempotencyContext
/// so both interfaces resolve to the same scoped instance.
/// </summary>
public interface IIdempotencyContextSetter
{
    void Set(Guid idempotencyKey, string handlerName);
}
