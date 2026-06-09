using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atlas.Identity.Infrastructure.Persistence.DbContexts;

public sealed class IdentityDbContextFactory
    : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ATLAS_IDENTITY_CONNECTION")
            ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=atlas;Username=atlas;Password=atlas_dev_password";

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new IdentityDbContext(options, new DesignTimeRequestContext());
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