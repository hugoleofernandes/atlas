namespace Atlas.SharedKernel.Application;

public abstract class OutboxMessageBase
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public string Name { get; protected set; } = default!;

    public string Type { get; protected set; } = default!;

    public string Payload { get; protected set; } = default!;

    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;

    public DateTime? ProcessedOn { get; protected set; }

    //public bool IsProcessed { get; protected set; }

    public Guid? TenantId { get; protected set; }

    public Guid? UserId { get; protected set; }

    public string? CorrelationId { get; protected set; }
}