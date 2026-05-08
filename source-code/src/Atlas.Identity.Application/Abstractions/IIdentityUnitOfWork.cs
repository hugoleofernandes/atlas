using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Abstractions;

public interface IIdentityUnitOfWork : IUnitOfWork
{
    Task AddOutboxMessage(OutboxMessage message);
}