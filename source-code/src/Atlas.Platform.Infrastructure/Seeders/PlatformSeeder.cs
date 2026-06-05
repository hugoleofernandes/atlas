using Atlas.BuildingBlocks.Application.Seeding;
using Atlas.Platform.Application.Abstractions;
using Atlas.Platform.Domain.Modules;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AtlasSystem = Atlas.Platform.Domain.Systems.AtlasSystem;

namespace Atlas.Platform.Infrastructure.Seeders;

/// <summary>
/// Seeds the Platform module registry: Systems, Modules, and EntityTypes.
/// Module registrations are injected via IModuleRegistration — each module's
/// Contracts project registers one implementation in Atlas.API.
/// Platform stays decoupled from other modules.
/// Idempotent — skips if data already exists.
/// </summary>
internal sealed class PlatformModuleSeeder : IModuleSeeder
{
    public int Order => 0;

    public async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        await new PlatformTenantSeeder().SeedAsync(services, ct);

        var logger        = services.GetRequiredService<ILogger<PlatformModuleSeeder>>();
        var db            = services.GetRequiredService<PlatformDbContext>();
        var uow           = services.GetRequiredService<IPlatformUnitOfWork>();
        var setter        = services.GetRequiredService<IRequestContextSetter>();
        var registrations = services.GetServices<IModuleRegistration>().ToList();

        if (await db.Modules.AnyAsync(ct))
        {
            logger.LogInformation("PlatformSeeder skipped — data already exists");
            return;
        }

        logger.LogInformation("PlatformSeeder started");

        db.Systems.Add(AtlasSystem.Create("mlab"));

        foreach (var registration in registrations)
        {
            var module = Module.Create(registration.ModuleId, registration.ModuleName);
            db.Modules.Add(module);

            foreach (var entityType in registration.EntityTypes)
                db.EntityTypes.Add(EntityType.Create(entityType.Id, module.Id, entityType.Name, entityType.Schema));
        }

        setter.Set(Guid.NewGuid(), "platform-seed", SystemIdentity.UserId, SystemIdentity.Email);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("PlatformSeeder completed — {Count} modules seeded", registrations.Count);
    }
}
