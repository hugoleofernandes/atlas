using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atlas.Staff.Infrastructure.Persistence.DbContexts;

public sealed class StaffDbContextFactory
    : IDesignTimeDbContextFactory<StaffDbContext>
{
    public StaffDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ATLAS_STAFF_CONNECTION")
            ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=atlas;Username=atlas;Password=atlas_dev_password";

        var options = new DbContextOptionsBuilder<StaffDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new StaffDbContext(options, new DesignTimeRequestContext());
    }

    private sealed class DesignTimeRequestContext : IRequestContext
    {
        public bool IsAuthenticated       => false;
        public Guid? TenantId             => null;
        public string? TenantName         => null;
        public Guid? UserId               => null;
        public string? UserEmail          => null;
        public string? CorrelationId      => null;
        public bool TenantFilterSuspended => false;
    }
}