using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence;

public abstract class MultiTenantDbContext : DbContext
{
    private readonly Guid? _tenantId;

    protected MultiTenantDbContext(
        DbContextOptions options,
        IRequestContext requestContext)
        : base(options)
    {
        _tenantId = requestContext?.TenantId;
    }

    protected Guid? CurrentTenantId => _tenantId;

    protected virtual string Schema => "atlas";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.Ignore<DomainEvent>();

        ApplyMultiTenantFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        if (_tenantId is null)
            return base.SaveChangesAsync(cancellationToken);

        foreach (var entry in ChangeTracker.Entries<IMultiTenantEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetTenantId(_tenantId.Value);
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyMultiTenantFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IMultiTenantEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var method = typeof(MultiTenantDbContext)
                .GetMethod(nameof(SetTenantFilter),
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(this, new object[] { modelBuilder });
        }
    }

    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, IMultiTenantEntity
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e =>
                !CurrentTenantId.HasValue || e.TenantId == CurrentTenantId.Value);
    }

    public IEnumerable<DomainEvent> GetDomainEvents()
    {
        return ChangeTracker
            .Entries<BaseEntity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();
    }

    public void ClearDomainEvents()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            entry.Entity.ClearDomainEvents();
    }

}