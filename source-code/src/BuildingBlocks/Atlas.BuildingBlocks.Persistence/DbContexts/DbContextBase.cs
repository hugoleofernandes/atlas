using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain;
using Atlas.SharedKernel.Domain.Events;
using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence.DbContexts;

public abstract class DbContextBase : DbContext
{
    private readonly IRequestContext _requestContext;

    protected Guid? CurrentTenantId => _requestContext.TenantId;

    private Guid CurrentTenantIdOrThrow =>
        _requestContext.TenantFilterSuspended
            ? Guid.Empty
            : _requestContext.TenantId ?? throw new InvalidOperationException(
                "A multi-tenant query was executed without a TenantId in the request context. " +
                "This is a bug — populate IRequestContextSetter before querying multi-tenant entities, " +
                "or call SuspendTenantFilter() for intentional cross-tenant access (e.g. bootstrap).");

    protected virtual string Schema => "atlas";

    protected DbContextBase(
        DbContextOptions options,
        IRequestContext requestContext)
        : base(options)
    {
        _requestContext = requestContext;

        // Disable automatic change detection on every LINQ call (Entries, Find, etc).
        // This avoids scanning all tracked entities on each read operation.
        // SaveChangesAsync is overridden below to always detect before committing,
        // so correctness is preserved regardless of how save is called.
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Always detect changes before saving, even with AutoDetectChangesEnabled = false.
        // This ensures correctness whether SaveChangesAsync is called via UoW or directly.
        ChangeTracker.DetectChanges();
        return base.SaveChangesAsync(ct);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.Ignore<IDomainEvent>();

        ApplyMultiTenantFilters(modelBuilder);
        modelBuilder.ValidateAuditableAggregates();

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

            // In a TPH hierarchy, derived types (e.g. Person, Organization) are separate
            // IEntityType entries sharing the root's table. EF Core only allows a query filter
            // on the hierarchy root — derived types inherit it automatically.
            if (entityType.BaseType is not null)
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
                _requestContext.TenantFilterSuspended
                || e.TenantId == CurrentTenantIdOrThrow);
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
