using Atlas.BuildingBlocks.Persistence;
using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.BuildingBlocks.Persistence.Entities.Idempotency;
using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Tenants;
using Atlas.Identity.Domain.Tenants._Roles;
using Atlas.Identity.Domain.Users;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.SharedKernel.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Persistence.DbContexts;

public sealed class IdentityDbContext
    : DbContextBase
{
    protected override string Schema => "atlas_identity";


    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Audit>                  Audits                 => Set<Audit>();
    public DbSet<OutboxMessage>          OutboxMessages         => Set<OutboxMessage>();
    public DbSet<OutboxHandlerExecution> OutboxHandlerExecutions => Set<OutboxHandlerExecution>();
    public DbSet<IdempotencyEntry>       IdempotencyEntries     => Set<IdempotencyEntry>();

    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        IRequestContext requestContext)
        : base(options, requestContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityInfrastructureAssemblyMarker).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PersistenceAssemblyMarker).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
