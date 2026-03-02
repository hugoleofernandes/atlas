using Atlas.BuildingBlocks.Persistence;
using Atlas.SharedKernel.Application;
using Atlas.Staff.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Staff.Infrastructure.Persistence;

public sealed class StaffDbContext
    : AuditableDbContext<StaffAuditLog>
{
    protected override string Schema => "atlas";
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<StaffAuditLog> AuditLogs => Set<StaffAuditLog>();

    public StaffDbContext(
        DbContextOptions<StaffDbContext> options,
        IRequestContext requestContext)
        : base(options, requestContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(StaffDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}