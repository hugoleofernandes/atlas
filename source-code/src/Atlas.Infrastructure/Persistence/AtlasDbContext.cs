//using Atlas.Application.Tenancy;
//using Atlas.Domain.Common;
//using Atlas.Domain.Identity;
//using Microsoft.EntityFrameworkCore;
//using System.Linq.Expressions;

//namespace Atlas.Infrastructure.Persistence;

//public class AtlasDbContext : DbContext
//{
//    // ef core tools require this constructor signature
//    public AtlasDbContext(DbContextOptions<AtlasDbContext> options) 
//        : base(options)
//    {
//    }

//    public DbSet<User> Users => Set<User>();
//    public DbSet<Tenant> Tenants => Set<Tenant>();
//    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtlasDbContext).Assembly);
//        base.OnModelCreating(modelBuilder);
//    }
//}



using System.Linq.Expressions;
using Atlas.Application.Tenancy;
using Atlas.Domain.Common;
using Atlas.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence;

public class AtlasDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public AtlasDbContext(DbContextOptions<AtlasDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();

    public DbSet<TestEntity> TestEntities => Set<TestEntity>();


    // Exponha TenantId como propriedade do DbContext (EF gosta disso para QueryFilter)
    public Guid CurrentTenantId => _tenantProvider.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtlasDbContext).Assembly);

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

            // e => ((IMultiTenantEntity)e).TenantId == CurrentTenantId
            var parameter = Expression.Parameter(clrType, "e");
            var tenantIdProperty = Expression.Property(
                Expression.Convert(parameter, typeof(IMultiTenantEntity)),
                nameof(IMultiTenantEntity.TenantId)
            );

            var currentTenantId = Expression.Property(
                Expression.Constant(this),
                nameof(CurrentTenantId)
            );

            var body = Expression.Equal(tenantIdProperty, currentTenantId);
            var lambda = Expression.Lambda(body, parameter);

            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyTenantIdToNewEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyTenantIdToNewEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyTenantIdToNewEntities()
    {
        var tenantId = CurrentTenantId; // lança se não setado

        foreach (var entry in ChangeTracker.Entries<IMultiTenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // evita “esqueci de setar”
                entry.Entity.TenantId = tenantId;
            }
        }
    }
}