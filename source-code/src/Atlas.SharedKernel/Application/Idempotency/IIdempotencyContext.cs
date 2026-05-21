namespace Atlas.SharedKernel.Application.Idempotency;

/// <summary>
/// Holds the idempotency key for the integration event handler currently executing.
/// Populated by OutboxMessageDispatcher before each handler invocation:
///   - IdempotencyKey = OutboxMessage.IdempotencyKey  (stable across retries)
///   - HandlerName    = handler type name (each handler tracks its own state)
/// </summary>
public interface IIdempotencyContext
{
    Guid   IdempotencyKey { get; }
    string HandlerName    { get; }
}
