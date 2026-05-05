using MediatR;

namespace Atlas.SharedKernel.Domain;

public abstract class DomainEvent : INotification
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}