using Atlas.BuildingBlocks.Application.Seeding;
using Atlas.Platform.Application.Abstractions;
using Atlas.Platform.Domain.Modules;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Atlas.Platform.Infrastructure.Seeders.Discovery;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AtlasSystem = Atlas.Platform.Domain.Systems.AtlasSystem;

namespace Atlas.Platform.Infrastructure.Seeders;

/// <summary>
/// Seeds the Platform module registry: Systems, Modules, and EntityTypes.
/// Metadata is discovered from Atlas.SharedKernel by scanning AtlasModule
/// and AtlasEntityType static fields.
/// Idempotent - skips if data already exists.
/// </summary>
internal sealed class PlatformModuleSeeder(
    IAtlasModuleDiscovery moduleDiscovery,
    IAtlasEntityTypeDiscovery entityTypeDiscovery
) : IModuleSeeder
{
    public int Order => 0;

    public async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        await new PlatformTenantSeeder().SeedAsync(services, ct);

        var logger = services.GetRequiredService<ILogger<PlatformModuleSeeder>>();
        var db = services.GetRequiredService<PlatformDbContext>();
        var uow = services.GetRequiredService<IPlatformUnitOfWork>();
        var setter = services.GetRequiredService<IRequestContextSetter>();
        var modules = moduleDiscovery.Discover();
        var entityTypes = entityTypeDiscovery.Discover();

        if (await db.Modules.AnyAsync(ct))
        {
            logger.LogInformation("PlatformSeeder skipped - data already exists");
            return;
        }

        logger.LogInformation("PlatformSeeder started");

        db.Systems.Add(AtlasSystem.Create("mlab"));

        foreach (var discoveredModule in modules)
        {
            var module = Module.Create(discoveredModule.Id, discoveredModule.Name);
            db.Modules.Add(module);

            foreach (var entityType in entityTypes.Where(x => x.Module.Id == discoveredModule.Id))
                db.EntityTypes.Add(EntityType.Create(entityType.Id, module.Id, entityType.Name));
        }

        setter.Set(Guid.NewGuid(), "platform-seed", SystemIdentity.UserId, SystemIdentity.Email);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("PlatformSeeder completed - {Count} modules seeded", modules.Count);
    }
}
