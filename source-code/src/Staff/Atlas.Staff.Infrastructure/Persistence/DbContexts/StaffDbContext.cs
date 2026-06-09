using Atlas.BuildingBlocks.Persistence;
using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.BuildingBlocks.Persistence.Entities.Idempotency;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.Staff.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Staff.Infrastructure.Persistence.DbContexts;

public sealed class StaffDbContext : DbContextBase
{
    protected override string Schema => "atlas_staff";

    public DbSet<StaffMember>       StaffMembers        => Set<StaffMember>();
    public DbSet<Audit>                  Audits                  => Set<Audit>();
    public DbSet<OutboxMessage>          OutboxMessages          => Set<OutboxMessage>();
    public DbSet<OutboxHandlerExecution> OutboxHandlerExecutions => Set<OutboxHandlerExecution>();
    public DbSet<IdempotencyEntry>       IdempotencyEntries      => Set<IdempotencyEntry>();

    public StaffDbContext(
        DbContextOptions<StaffDbContext> options,
        IRequestContext requestContext)
        : base(options, requestContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StaffInfrastructureAssemblyMarker).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PersistenceAssemblyMarker).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
