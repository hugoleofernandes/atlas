using Atlas.BuildingBlocks.Persistence;
using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.BuildingBlocks.Persistence.Entities.Idempotency;
using Atlas.Party.Domain.Parties;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Party.Infrastructure.Persistence.DbContexts;

public sealed class PartyDbContext : DbContextBase
{
    protected override string Schema => "atlas_party";

    public DbSet<Domain.Parties.Party> Parties => Set<Domain.Parties.Party>();
    public DbSet<Individual> Individuals => Set<Individual>();
    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<Audit> Audits => Set<Audit>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<OutboxHandlerExecution> OutboxHandlerExecutions => Set<OutboxHandlerExecution>();
    public DbSet<IdempotencyEntry> IdempotencyEntries => Set<IdempotencyEntry>();

    public PartyDbContext(DbContextOptions<PartyDbContext> options, IRequestContext requestContext)
        : base(options, requestContext) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PartyInfrastructureAssemblyMarker).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PersistenceAssemblyMarker).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
