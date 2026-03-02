using Atlas.Identity.Domain.Entities;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Atlas.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<IdentityUser> IdentityUsers => Set<IdentityUser>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();

    public Guid CurrentTenantId => _tenantProvider.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("atlas");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IdentityDbContext).Assembly);

        ApplyMultiTenantFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private void ApplyMultiTenantFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (!typeof(IMultiTenantEntity).IsAssignableFrom(clrType))
                continue;

            var parameter = Expression.Parameter(clrType, "e");

            var tenantIdProperty = Expression.Property(
                Expression.Convert(parameter, typeof(IMultiTenantEntity)),
                nameof(IMultiTenantEntity.TenantId));

            var currentTenantId = Expression.Property(
                Expression.Constant(this),
                nameof(CurrentTenantId));

            var body = Expression.Equal(tenantIdProperty, currentTenantId);

            var lambda = Expression.Lambda(body, parameter);

            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyTenantIdToNewEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTenantIdToNewEntities()
    {
        Guid tenantId;

        try
        {
            tenantId = _tenantProvider.TenantId;
        }
        catch
        {
            return;
        }

        foreach (var entry in ChangeTracker.Entries<IMultiTenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.TenantId = tenantId;
            }
        }
    }
}