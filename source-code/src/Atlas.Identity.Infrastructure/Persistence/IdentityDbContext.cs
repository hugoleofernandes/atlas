using Atlas.BuildingBlocks.Persistence;
using Atlas.Identity.Domain.Tenants;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext
    : AuditableDbContext<UserAuditLog>
{
    protected override string Schema => "atlas_identity";

    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Invitation> Invitations => Set<Invitation>();

    public DbSet<UserAuditLog> AuditLogs => Set<UserAuditLog>();

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