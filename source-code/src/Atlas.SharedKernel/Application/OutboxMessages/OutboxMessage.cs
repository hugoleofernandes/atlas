using Atlas.SharedKernel.Domain;

namespace Atlas.SharedKernel.Application.OutboxMessages;

/// <summary>
/// One processing attempt for an integration event.
///
/// Implements INotAuditable — outbox messages are infrastructure,
/// not business data, and should not generate audit log entries.
///
/// Retry model — Attempt-Chain
/// ───────────────────────────
/// Each row is immutable once closed. On failure a new child row is created
/// (same IdempotencyKey, AttemptNumber + 1). The full per-handler execution
/// history is stored in outbox_handler_executions (one row per handler per attempt).
///
/// Attempt flow:
///   Pending  →  MarkAsProcessed()        (all handlers OK)
///   Pending  →  CreateRetryAttempt()     (some handlers failed, maxAttempts not reached)
///               └─ closes this row (FailedAt set) and returns the next attempt row
///   Pending  →  MarkAsDeadLettered()     (some handlers failed, maxAttempts reached)
///
/// Pending batch query: ProcessedOn IS NULL AND DeadLetteredOn IS NULL AND FailedAt IS NULL
///
/// // TODO: add a background cleanup job that archives or deletes rows older than N days
///          (e.g. WHERE processed_on < NOW() - INTERVAL '30 days').
///          Skipped for now — implement when volume warrants it.
/// </summary>
public sealed class OutboxMessage : INotAuditable
{
    public Guid Id { get; private set; }

    /// <summary>
    /// Stable identifier shared across all retry attempts for the same logical event.
    /// Set to <see cref="Id"/> on the first attempt; copied from the parent on retries.
    /// Handlers use this key for idempotency — if a handler already processed
    /// this key it skips execution, even on a new retry row.
    /// </summary>
    public Guid IdempotencyKey { get; private set; }

    /// <summary>
    /// Id of the previous attempt row. Null on the first attempt.
    /// Allows navigating the full attempt chain: C → B → A.
    /// </summary>
    public Guid? ParentOutboxMessageId { get; private set; }

    /// <summary>
    /// 1-based counter. First attempt = 1, first retry = 2, etc.
    /// Used to decide whether to spawn another retry or dead-letter.
    /// </summary>
    public int AttemptNumber { get; private set; }

    public string Name { get; private set; } = default!;

    public string Type { get; private set; } = default!;

    public string Payload { get; private set; } = default!;

    public DateTime OccurredOn { get; private set; }

    public DateTime? ProcessedOn { get; private set; }

    public Guid TenantId { get; private set; }

    public Guid UserId { get; private set; }

    public string CorrelationId { get; private set; } = default!;

    public string Module { get; private set; } = default!;

    /// <summary>
    /// Brief human-readable summary of the failure. Null on success.
    /// Full per-handler detail is in the outbox_handler_executions table.
    /// </summary>
    public string? Error { get; private set; }

    public Guid? LockId { get; private set; }

    public DateTime? LockedUntil { get; private set; }

    /// <summary>
    /// Set when this attempt fails and a child retry row has been created.
    /// A row with FailedAt set is excluded from the pending batch query.
    /// </summary>
    public DateTime? FailedAt { get; private set; }

    public DateTime? DeadLetteredOn { get; private set; }

    public bool IsProcessed    => ProcessedOn.HasValue;
    public bool IsDeadLettered => DeadLetteredOn.HasValue;
    public bool IsFailed       => FailedAt.HasValue;

    // -------------------------
    // CONSTRUCTORS
    // -------------------------

    private OutboxMessage() { }

    /// <summary>Creates the first attempt (AttemptNumber = 1).</summary>
    public OutboxMessage(
        string name,
        string type,
        string payload,
        Guid   tenantId,
        Guid   userId,
        string correlationId,
        string module)
    {
        Id             = Guid.NewGuid();
        IdempotencyKey = Id;
        AttemptNumber  = 1;
        Name           = name;
        Type           = type;
        Payload        = payload;
        TenantId       = tenantId;
        UserId         = userId;
        CorrelationId  = correlationId;
        Module         = module;
        OccurredOn     = DateTime.UtcNow;
    }

    /// <summary>
    /// Private — called only by <see cref="CreateRetryAttempt"/>.
    /// Creates the next attempt row copying all event data from the parent.
    /// </summary>
    private OutboxMessage(
        Guid   parentId,
        Guid   idempotencyKey,
        int    attemptNumber,
        string name,
        string type,
        string payload,
        Guid   tenantId,
        Guid   userId,
        string correlationId,
        string module)
    {
        Id                    = Guid.NewGuid();
        IdempotencyKey        = idempotencyKey;
        ParentOutboxMessageId = parentId;
        AttemptNumber         = attemptNumber;
        Name                  = name;
        Type                  = type;
        Payload               = payload;
        TenantId              = tenantId;
        UserId                = userId;
        CorrelationId         = correlationId;
        Module                = module;
        OccurredOn            = DateTime.UtcNow;
    }

    // -------------------------
    // PROCESSING
    // -------------------------

    /// <summary>
    /// Closes this attempt as fully successful.
    /// Handler execution records are persisted separately via IOutboxWorkerRepository.AddExecutionsAsync.
    /// </summary>
    public void MarkAsProcessed()
    {
        ProcessedOn = DateTime.UtcNow;
        ClearLock();
    }

    /// <summary>
    /// Closes this attempt as failed and returns the next attempt row ready to be persisted.
    /// The two operations happen in the same Unit of Work transaction —
    /// either both are committed or neither.
    /// Handler execution records are persisted separately via IOutboxWorkerRepository.AddExecutionsAsync.
    /// <paramref name="errorSummary"/> is an optional short description for quick debugging
    /// without a join to outbox_handler_executions.
    /// </summary>
    public OutboxMessage CreateRetryAttempt(string? errorSummary = null)
    {
        FailedAt = DateTime.UtcNow;
        Error    = errorSummary;
        ClearLock();

        return new OutboxMessage(
            parentId:       Id,
            idempotencyKey: IdempotencyKey,
            attemptNumber:  AttemptNumber + 1,
            name:           Name,
            type:           Type,
            payload:        Payload,
            tenantId:       TenantId,
            userId:         UserId,
            correlationId:  CorrelationId,
            module:         Module);
    }

    /// <summary>
    /// Closes this attempt as dead-lettered (max attempts reached, no more retries).
    /// Handler execution records are persisted separately via IOutboxWorkerRepository.AddExecutionsAsync.
    /// </summary>
    public void MarkAsDeadLettered()
    {
        DeadLetteredOn = DateTime.UtcNow;
        ClearLock();
    }

    // -------------------------
    // LOCK (OPTIMISTIC)
    // -------------------------

    public bool TryLock(Guid lockId, TimeSpan duration)
    {
        if (IsLocked())
            return false;

        LockId      = lockId;
        LockedUntil = DateTime.UtcNow.Add(duration);
        return true;
    }

    public bool IsLocked()
        => LockedUntil.HasValue && LockedUntil > DateTime.UtcNow;

    private void ClearLock()
    {
        LockId      = null;
        LockedUntil = null;
    }

    // -------------------------
    // ATTEMPT RULES
    // -------------------------

    /// <summary>
    /// True when this attempt number has reached or exceeded the configured maximum.
    /// When true the worker dead-letters instead of spawning another retry.
    /// </summary>
    public bool IsMaxAttemptReached(int maxRetries)
        => AttemptNumber >= maxRetries;
}