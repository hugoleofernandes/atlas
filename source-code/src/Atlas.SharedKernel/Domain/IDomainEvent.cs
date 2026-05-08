//using MediatR;

namespace Atlas.SharedKernel.Domain;

public interface IDomainEvent //: INotification
{
    DateTime OccurredOn { get; }
}