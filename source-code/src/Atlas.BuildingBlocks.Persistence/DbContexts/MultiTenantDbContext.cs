using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain;
using Atlas.SharedKernel.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence.DbContexts;

public abstract class MultiTenantDbContext : DbContext
{
    private readonly IRequestContext _requestContext;

    protected MultiTenantDbContext(
        DbContextOptions options,
        IRequestContext requestContext)
        : base(options)
    {
        _requestContext = requestContext;
    }

    protected Guid? CurrentTenantId => _requestContext?.TenantId;

    protected virtual string Schema => "atlas";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.Ignore<IDomainEvent>();

        ApplyMultiTenantFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditableEntityFields();
        SetMultiTenantFields();

        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditableEntityFields()
    {
        var now       = DateTime.UtcNow;
        var userId    = _requestContext?.UserId;
        var userEmail = _requestContext?.UserEmail;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetCreated(now, userId, userEmail);

            else if (entry.State == EntityState.Modified)
                entry.Entity.SetUpdated(now, userId, userEmail);
        }
    }

    private void SetMultiTenantFields()
    {
        if (_requestContext?.TenantId is not { } tenantId)
            return;

        foreach (var entry in ChangeTracker.Entries<IMultiTenantEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetTenantId(tenantId);
        }
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

    public IEnumerable<IDomainEvent> GetDomainEvents()
    {
        return ChangeTracker
            .Entries<AggregateRootBase>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();
    }

    public void ClearDomainEvents()
    {
        foreach (var entry in ChangeTracker.Entries<AggregateRootBase>())
            entry.Entity.ClearDomainEvents();
    }
}
