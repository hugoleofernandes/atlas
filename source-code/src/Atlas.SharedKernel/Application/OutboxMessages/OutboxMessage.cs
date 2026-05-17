using Atlas.SharedKernel.Domain;

namespace Atlas.SharedKernel.Application.OutboxMessages;

/// <summary>
/// Implements INotAuditable — outbox messages are infrastructure,
/// not business data, and should not generate audit log entries.
/// </summary>
public sealed class OutboxMessage : INotAuditable
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = default!;

    public string Type { get; private set; } = default!;

    public string Payload { get; private set; } = default!;

    public DateTime OccurredOn { get; private set; }

    public DateTime? ProcessedOn { get; private set; }

    public Guid? TenantId { get; private set; }

    public Guid? UserId { get; private set; }

    public string? CorrelationId { get; private set; }

    public string Module { get; private set; } = default!;

    public int RetryCount { get; private set; }

    public string? Error { get; private set; }

    public Guid? LockId { get; private set; }

    public DateTime? LockedUntil { get; private set; }

    public DateTime? DeadLetteredOn { get; private set; }

    public bool IsProcessed => ProcessedOn.HasValue;

    public bool IsDeadLettered => DeadLetteredOn.HasValue;

    //NextRetryAt
    //ProcessedBy

    private OutboxMessage() { }

    public OutboxMessage(
        string name,
        string type,
        string payload,
        Guid? tenantId,
        Guid? userId,
        string? correlationId,
        string module)
    {
        Id = Guid.NewGuid();

        Name = name;
        Type = type;
        Payload = payload;

        TenantId = tenantId;
        UserId = userId;
        CorrelationId = correlationId;
        Module = module;

        OccurredOn = DateTime.UtcNow;
    }

    // -------------------------
    // PROCESSING
    // -------------------------

    public void MarkAsProcessed()
    {
        ProcessedOn = DateTime.UtcNow;
        ClearLock();
    }

    public void MarkAsFailed(string error)
    {
        RetryCount++;
        Error = error;
        ClearLock();
    }

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

        LockId = lockId;
        LockedUntil = DateTime.UtcNow.Add(duration);

        return true;
    }

    public bool IsLocked()
        => LockedUntil.HasValue && LockedUntil > DateTime.UtcNow;

    private void ClearLock()
    {
        LockId = null;
        LockedUntil = null;
    }

    // -------------------------
    // RETRY RULES
    // -------------------------

    public bool HasExceededRetries(int maxRetries)
        => RetryCount >= maxRetries;

    public bool CanBeProcessed(int maxRetries)
    {
        if (IsProcessed || IsDeadLettered)
            return false;

        if (HasExceededRetries(maxRetries))
            return false;

        if (IsLocked())
            return false;

        return true;
    }
}