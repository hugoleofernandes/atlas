using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Atlas.BuildingBlocks.Persistence;

public abstract class MultiTenantDbContext : DbContext
{
    private readonly Guid _tenantId;

    protected MultiTenantDbContext(
        DbContextOptions options,
        IRequestContext requestContext)
        : base(options)
    {
        if (requestContext.TenantId is null)
            throw new InvalidOperationException("TenantId is required.");

        _tenantId = requestContext.TenantId.Value;
    }

    protected Guid CurrentTenantId => _tenantId;

    protected virtual string Schema => "atlas";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        ApplyMultiTenantFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyTenantIdToNewEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTenantIdToNewEntities()
    {
        foreach (var entry in ChangeTracker.Entries<IMultiTenantEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetTenantId(_tenantId);
        }
    }

    private void ApplyMultiTenantFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IMultiTenantEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");

            var tenantProperty = Expression.Property(
                Expression.Convert(parameter, typeof(IMultiTenantEntity)),
                nameof(IMultiTenantEntity.TenantId));

            var tenantField = Expression.Field(
                Expression.Constant(this),
                nameof(_tenantId));

            var equalsTenant = Expression.Equal(
                tenantProperty,
                tenantField);

            var lambda = Expression.Lambda(equalsTenant, parameter);

            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(lambda);
        }
    }
}