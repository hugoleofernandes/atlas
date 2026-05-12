namespace Atlas.SharedKernel.Domain.Events;

public interface IDomainEvent
{
    Guid TenantId { get; }
    //Guid UserId { get; }
    DateTime OccurredOn { get; }
}