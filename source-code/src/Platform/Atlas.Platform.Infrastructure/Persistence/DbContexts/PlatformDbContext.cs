using Atlas.BuildingBlocks.Persistence;
using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.Platform.Domain.Geography;
using Atlas.Platform.Domain.Modules;
using Atlas.Platform.Domain.Tenants;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using AtlasSystem = Atlas.Platform.Domain.Systems.AtlasSystem;

namespace Atlas.Platform.Infrastructure.Persistence.DbContexts;

public sealed class PlatformDbContext : DbContextBase
{
    protected override string Schema => "atlas_platform";

    public DbSet<AtlasSystem> Systems     => Set<AtlasSystem>();
    public DbSet<Module>      Modules     => Set<Module>();
    public DbSet<EntityType>  EntityTypes => Set<EntityType>();
    public DbSet<Tenant>      Tenants     => Set<Tenant>();
    public DbSet<State>       States      => Set<State>();
    public DbSet<City>        Cities      => Set<City>();

    public PlatformDbContext(
        DbContextOptions<PlatformDbContext> options,
        IRequestContext requestContext)
        : base(options, requestContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlatformInfrastructureAssemblyMarker).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PersistenceAssemblyMarker).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
