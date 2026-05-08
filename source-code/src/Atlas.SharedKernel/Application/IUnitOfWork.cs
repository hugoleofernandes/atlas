namespace Atlas.SharedKernel.Application;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);

    Task<T> GetDbContext<T> () where T : class;
}