namespace Atlas.SharedKernel.Domain;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}