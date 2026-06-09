namespace Atlas.SharedKernel.Application.Idempotency;

/// <summary>
/// Provides idempotency protection for integration event handlers.
///
/// Call HasAlreadyProcessedAsync at the start of HandleAsync.
/// If it returns true the message was already handled — return early.
/// If it returns false the key was atomically marked as processed — proceed with business logic.
///
/// Current implementation: PostgreSQL via INSERT ON CONFLICT DO NOTHING (atomic, no race condition).
/// Future implementation: Redis SET NX EX (sub-millisecond, TTL built-in, no cleanup job needed).
/// Swapping storage requires only a new implementation class + DI registration change.
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Atomically checks whether the current (MessageId, HandlerName) was already processed
    /// and, if not, records it as processed.
    ///
    /// Returns <c>true</c>  → already processed, handler must return early (skip).
    /// Returns <c>false</c> → first time, handler must proceed with business logic.
    /// </summary>
    Task<bool> HasAlreadyProcessedAsync(CancellationToken ct);
}
