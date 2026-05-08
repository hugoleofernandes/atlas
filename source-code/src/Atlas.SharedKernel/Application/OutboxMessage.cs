namespace Atlas.SharedKernel.Application;

public sealed class OutboxMessage : OutboxMessageBase
{
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
        Name = name;
        Type = type;
        Payload = payload;

        TenantId = tenantId;
        UserId = userId;
        CorrelationId = correlationId;

        Module = module;
    }

    public string Module { get; private set; } = default!;

    public void MarkAsProcessed()
    {
        ProcessedOn = DateTime.UtcNow;
    }
}