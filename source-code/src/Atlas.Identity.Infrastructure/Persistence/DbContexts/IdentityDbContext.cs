using Atlas.BuildingBlocks.Persistence;
using Atlas.BuildingBlocks.Persistence.OutboxMessages;
using Atlas.Identity.Domain.Entities.Audits;
using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Persistence.DbContexts;

public sealed class IdentityDbContext
    : MultiTenantDbContext
{
    protected override string Schema => "atlas_identity";

    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Invitation> Invitations => Set<Invitation>();

    public DbSet<IdentityModuleAudit> IdentityModuleAudit => Set<IdentityModuleAudit>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        IRequestContext requestContext)
        : base(options, requestContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxMessageConfiguration).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}