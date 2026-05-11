namespace Atlas.SharedKernel.Application.IntegrationEvents;

public sealed class OutboxMessage
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

    public bool IsProcessed => ProcessedOn.HasValue;

    public void MarkAsProcessed()
    {
        ProcessedOn = DateTime.UtcNow;
    }


    private OutboxMessage()
    {
    }

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
}