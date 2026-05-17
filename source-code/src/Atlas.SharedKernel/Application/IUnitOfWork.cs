using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
    IEnumerable<IDomainEvent> GetDomainEvents();
}