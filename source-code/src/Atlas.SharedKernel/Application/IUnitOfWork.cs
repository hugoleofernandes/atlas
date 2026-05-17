namespace Atlas.SharedKernel.Application;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}
