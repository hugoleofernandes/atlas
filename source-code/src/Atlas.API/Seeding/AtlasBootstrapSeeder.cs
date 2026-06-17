using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Users;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Identity.Infrastructure.Seeders;
using Atlas.Platform.Application.Queries.EntityTypes.Lookup;
using Atlas.Platform.Application.Queries.Geography;
using Atlas.Platform.Domain.Tenants;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Atlas.Platform.Infrastructure.Seeders;
using Atlas.SharedKernel.Application;
using Atlas.Staff.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;

namespace Atlas.API.Seeding;

/// <summary>
/// Orchestrates the complete bootstrap flow in a single coordinated sequence:
///   1. EnsureTenantAsync              — create/obtain the root tenant by deterministic ID
///   2. EnsureRootUserAsync            — create/obtain the root user by deterministic ID
///   3. Set request context            — tenantId + rootUserId for all subsequent seeders
///   4. PlatformModuleSeeder           — system, modules, entity types
///   5. IdentityPermissionCatalogSeeder — permissions
///   6. IdentityModuleSeeder           — roles, invitations, user role assignment
///   7. StaffModuleSeeder              — no-op currently
///
/// Tenant and root user IDs are deterministic (<see cref="BootstrapIdentity"/>),
/// so lookups are always by ID — no fuzzy queries.
/// All subsequent seeders inherit the initialized RequestContext.
/// </summary>
public sealed class AtlasBootstrapSeeder(
    IRequestContext requestContext,
    IRequestContextSetter requestContextSetter,
    PlatformModuleSeeder platformModuleSeeder,
    IdentityModuleSeeder identityModuleSeeder,
    IdentityPermissionCatalogSeeder catalogSeeder,
    StaffModuleSeeder staffModuleSeeder,
    IEntityTypeCatalogCache entityTypeCache,
    IGeographyCache geographyCache,
    PlatformDbContext platformDb,
    IdentityDbContext identityDb,
    IIdentityUnitOfWork identityUow,
    ILogger<AtlasBootstrapSeeder> logger
)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(requestContext.CorrelationId))
            requestContextSetter.SetCorrelationId(Guid.NewGuid().ToString());

        logger.LogInformation("Atlas bootstrap seeding started");

        // Set context once — IDs are deterministic (BootstrapIdentity), so this is safe
        // before the entities even exist. Every subsequent operation in this flow
        // inherits this context: tenant creation, root user creation, and all seeders.
        // This is the only place requestContextSetter.Set() is called in the bootstrap flow.
        requestContextSetter.Set(
            BootstrapIdentity.RootTenant.Id,
            BootstrapIdentity.RootTenant.Name,
            BootstrapIdentity.RootUser.Id,
            BootstrapIdentity.RootUser.Email
        );

        await EnsureTenantAsync(ct);
        await EnsureRootUserAsync(ct);

        var allModules = new[]
        {
            platformModuleSeeder.GetModule(),
            identityModuleSeeder.GetModule(),
            staffModuleSeeder.GetModule(),
        };

        var allEntityTypes = new[]
        {
            platformModuleSeeder.GetModuleEntityTypes(),
            identityModuleSeeder.GetModuleEntityTypes(),
            staffModuleSeeder.GetModuleEntityTypes(),
        };

        await platformModuleSeeder.SeedAsync(allModules, allEntityTypes, entityTypeCache, geographyCache, ct);

        var allPermissions = new[]
        {
            platformModuleSeeder.GetModulePermissions(),
            identityModuleSeeder.GetModulePermissions(),
            staffModuleSeeder.GetModulePermissions(),
        };
        await catalogSeeder.SeedAsync(allPermissions, ct);

        await identityModuleSeeder.SeedAsync(ct);

        await staffModuleSeeder.SeedAsync(ct);

        logger.LogInformation("Atlas bootstrap seeding completed");
    }

    private async Task EnsureTenantAsync(CancellationToken ct)
    {
        // Tenant is INotMultiTenant — no query filter is registered for it.
        // Neither SuspendTenantFilter nor IgnoreQueryFilters is needed.
        var existing = await platformDb.Tenants
            .FirstOrDefaultAsync(t => t.Id == BootstrapIdentity.RootTenant.Id, ct);

        if (existing is not null)
        {
            logger.LogInformation(
                "Bootstrap tenant: {TenantName} ({TenantId}) — already exists",
                existing.Name,
                existing.Id);
            return;
        }

        var tenant = Tenant.CreateForBootstrap();
        platformDb.Tenants.Add(tenant);
        await platformDb.SaveChangesAsync(ct);

        logger.LogInformation(
            "Bootstrap tenant: {TenantName} ({TenantId}) — created",
            tenant.Name,
            tenant.Id);
    }

    private async Task EnsureRootUserAsync(CancellationToken ct)
    {
        // IgnoreQueryFilters() bypasses the EF HasQueryFilter lambda entirely —
        // CurrentTenantIdOrThrow is never called, so SuspendTenantFilter is redundant here.
        var exists = await identityDb.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Id == BootstrapIdentity.RootUser.Id, ct);

        if (exists)
        {
            logger.LogInformation(
                "Bootstrap root user: {Email} ({UserId}) — already exists",
                BootstrapIdentity.RootUser.Email,
                BootstrapIdentity.RootUser.Id);
            return;
        }

        // Context is already set (RunAsync sets it once before calling here).
        // The audit stamper will use BootstrapIdentity.RootUser.Id as CreatedBy.
        var rootUser = User.CreateRootForBootstrap(
            BootstrapIdentity.RootTenant.Id,
            Email.Create(BootstrapIdentity.RootUser.Email));

        identityDb.Users.Add(rootUser);
        await identityUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Bootstrap root user: {Email} ({UserId}) — created",
            rootUser.Email.Value,
            rootUser.Id);
    }
}
