using Atlas.BuildingBlocks.Persistence;
using Atlas.Identity.Domain.Entities;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext
    : AuditableDbContext<IdentityAuditLog>
{
    protected override string Schema => "atlas_identity";

    public DbSet<IdentityUser> IdentityUsers => Set<IdentityUser>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();
    public DbSet<IdentityAuditLog> AuditLogs => Set<IdentityAuditLog>();


    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        IRequestContext requestContext)
        : base(options, requestContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IdentityDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}