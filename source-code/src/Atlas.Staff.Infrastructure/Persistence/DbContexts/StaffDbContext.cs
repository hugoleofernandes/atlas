using Atlas.BuildingBlocks.Persistence.Audits;
using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.Staff.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Staff.Infrastructure.Persistence.DbContexts;

public sealed class StaffDbContext
    : MultiTenantDbContext
{
    protected override string Schema => "atlas_staff";

    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<Audit> Audits => Set<Audit>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public StaffDbContext(
        DbContextOptions<StaffDbContext> options,
        IRequestContext requestContext)
        : base(options, requestContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StaffDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditConfiguration).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
