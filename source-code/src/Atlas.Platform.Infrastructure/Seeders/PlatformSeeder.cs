using Atlas.BuildingBlocks.Application.Seeding;
using Atlas.Platform.Application.Abstractions;
using Atlas.Platform.Domain.Modules;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AtlasSystem = Atlas.Platform.Domain.Systems.AtlasSystem;

namespace Atlas.Platform.Infrastructure.Seeders;

/// <summary>
/// Seeds the Platform module registry: Systems, Modules, and EntityTypes.
/// Idempotent — skips if data already exists.
/// </summary>
internal sealed class PlatformModuleSeeder : IModuleSeeder
{
    public int Order => 0;

    public async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        await new PlatformTenantSeeder().SeedAsync(services, ct);

        var logger = services.GetRequiredService<ILogger<PlatformModuleSeeder>>();
        var db     = services.GetRequiredService<PlatformDbContext>();
        var uow    = services.GetRequiredService<IPlatformUnitOfWork>();
        var setter = services.GetRequiredService<IRequestContextSetter>();

        if (await db.Modules.AnyAsync(ct))
        {
            logger.LogInformation("PlatformSeeder skipped — data already exists");
            return;
        }

        logger.LogInformation("PlatformSeeder started");

        // Systems
        var mlab = AtlasSystem.Create("mlab");
        db.Systems.Add(mlab);

        // Modules
        var identityModule = Module.Create("identity");
        var staffModule    = Module.Create("staff");
        var platformModule = Module.Create("platform");

        db.Modules.AddRange(identityModule, staffModule, platformModule);

        // EntityTypes — Identity module
        db.EntityTypes.AddRange(
            EntityType.Create(identityModule.Id, "User",       "atlas_identity"),
            EntityType.Create(identityModule.Id, "Role",       "atlas_identity"),
            EntityType.Create(identityModule.Id, "Invitation", "atlas_identity"),
            EntityType.Create(platformModule.Id, "Tenant",     "atlas_platform"));

        // EntityTypes — Staff module
        db.EntityTypes.Add(
            EntityType.Create(staffModule.Id, "StaffMember", "atlas_staff"));

        var systemId    = Guid.NewGuid();
        var systemEmail = SystemIdentity.Email;

        setter.Set(systemId, "platform-seed", SystemIdentity.UserId, systemEmail);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("PlatformSeeder completed:");
        logger.LogInformation("  Systems : {Systems}", mlab.Name);
        logger.LogInformation("  Modules : identity, staff, platform");
        logger.LogInformation("  EntityTypes : User, Role, Invitation, Tenant (identity) | StaffMember (staff)");
    }
}
