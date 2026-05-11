using Atlas.BuildingBlocks.Persistence;
using Atlas.BuildingBlocks.Persistence.Outbox;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.Staff.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Staff.Infrastructure.Persistence;

public sealed class StaffDbContext
    : MultiTenantDbContext
{
    protected override string Schema => "atlas_staff";
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<StaffAuditLog> AuditLogs => Set<StaffAuditLog>();

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

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxMessageConfiguration).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}