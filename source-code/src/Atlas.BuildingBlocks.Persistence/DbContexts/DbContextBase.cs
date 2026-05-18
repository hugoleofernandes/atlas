using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain;
using Atlas.SharedKernel.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence.DbContexts;

public abstract class DbContextBase : DbContext
{
    private readonly IRequestContext _requestContext;

    protected Guid? CurrentTenantId => _requestContext.TenantId;

    protected virtual string Schema => "atlas";

    protected DbContextBase(
        DbContextOptions options,
        IRequestContext requestContext)
        : base(options)
    {
        _requestContext = requestContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.Ignore<IDomainEvent>();

        ApplyMultiTenantFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private void ApplyMultiTenantFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IMultiTenantEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            if (typeof(INotMultiTenant).IsAssignableFrom(entityType.ClrType))
                continue;

            var method = typeof(DbContextBase)
                .GetMethod(nameof(SetTenantFilter),
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(this, [modelBuilder]);
        }
    }

    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, IMultiTenantEntity
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e =>
                !CurrentTenantId.HasValue || e.TenantId == CurrentTenantId.Value);
    }

    public IEnumerable<IDomainEvent> GetDomainEvents()
    {
        return ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();
    }

    public void ClearDomainEvents()
    {
        foreach (var entry in ChangeTracker.Entries<AggregateRoot>())
            entry.Entity.ClearDomainEvents();
    }
}
