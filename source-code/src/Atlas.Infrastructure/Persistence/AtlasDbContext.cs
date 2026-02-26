using Atlas.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence;

public class AtlasDbContext : DbContext
{
    // ef core tools require this constructor signature
    public AtlasDbContext(DbContextOptions<AtlasDbContext> options) 
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtlasDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}