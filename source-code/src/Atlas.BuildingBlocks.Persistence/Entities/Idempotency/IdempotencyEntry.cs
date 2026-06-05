using Atlas.SharedKernel.Domain;

namespace Atlas.BuildingBlocks.Persistence.Entities.Idempotency;

/// <summary>
/// Records that a specific handler successfully processed a specific outbox message.
/// The composite primary key (MessageId, HandlerName) acts as the unique constraint â€”
/// the INSERT ON CONFLICT pattern uses it to guarantee atomic check-and-mark.
///
/// Implements INotAuditable: infrastructure record, must not generate audit trail entries.
/// </summary>
public sealed class IdempotencyEntry : INotAuditable
{
    /// <summary>
    /// OutboxMessage.IdempotencyKey â€” stable across retries.
    /// Even when a retry generates a new OutboxMessage row (new Id),
    /// this value is copied from the original so the check still matches.
    /// </summary>
    public Guid IdempotencyKey { get; private set; }

    /// <summary>
    /// Handler type name (e.g. "CreateStaffMemberIntegrationEventHandler").
    /// Each handler tracks its own processed state independently.
    /// </summary>
    public string HandlerName { get; private set; } = default!;

    public DateTime ProcessedAt { get; private set; }

    private IdempotencyEntry() { }

    public IdempotencyEntry(Guid idempotencyKey, string handlerName)
    {
        IdempotencyKey = idempotencyKey;
        HandlerName    = handlerName;
        ProcessedAt    = DateTime.UtcNow;
    }
}
