using Atlas.SharedKernel.Domain;

namespace Atlas.SharedKernel.Application.OutboxMessages;

/// <summary>
/// Persisted record of a single handler invocation within one outbox attempt.
///
/// One row is created per handler per <see cref="OutboxMessage"/> attempt, regardless
/// of whether the handler succeeded, was skipped (idempotency hit), or failed.
///
/// Status values:
///   "Success"    — handler ran to completion or was skipped by idempotency (both are correct).
///   "Failure"    — handler threw or returned a failure result.
///   "Dispatcher" — recorded by the dispatcher on pre-handler errors
///                  (unknown event type, deserialization failure, no handlers registered).
///
/// Implements INotAuditable — execution records are infrastructure,
/// not business data, and must not generate audit log entries.
/// </summary>
public sealed class OutboxHandlerExecution : INotAuditable
{
    public Guid Id { get; private set; }

    /// <summary>
    /// The attempt row this execution belongs to.
    /// Cascade-deleted when the parent OutboxMessage is removed by the cleanup job.
    /// </summary>
    public Guid OutboxMessageId { get; private set; }

    public string HandlerName { get; private set; } = default!;

    /// <summary>"Success" | "Failure"</summary>
    public string Status { get; private set; } = default!;

    public string? ErrorMessage { get; private set; }

    public DateTime AttemptedAt { get; private set; }

    private OutboxHandlerExecution() { }

    public OutboxHandlerExecution(
        Guid    outboxMessageId,
        string  handlerName,
        bool    isSuccess,
        string? errorMessage = null)
    {
        Id              = Guid.NewGuid();
        OutboxMessageId = outboxMessageId;
        HandlerName     = handlerName;
        Status          = isSuccess ? "Success" : "Failure";
        ErrorMessage    = errorMessage;
        AttemptedAt     = DateTime.UtcNow;
    }
}
