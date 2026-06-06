using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Tenants._Roles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application;
using Atlas.Staff.Domain.ModulePermissions;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Infrastructure.Seeders.Aggregates;

/// <summary>
/// Seeds the default system roles (root, admin, member) for the tenant that lives in Atlas.Platform.
/// Reads atlas_platform.tenants via Dapper to avoid a cross-module project reference.
/// Idempotent â€” skips if any role already exists.
/// </summary>
internal sealed class IdentityRoleSeeder
{
    private const string GetFirstTenantSql = """
        SELECT id AS TenantId, name AS TenantName
        FROM atlas_platform.tenants
        ORDER BY created_at
        LIMIT 1
        """;

    private sealed record TenantRow(Guid TenantId, string TenantName);

    public async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        var logger = services.GetRequiredService<ILogger<IdentityRoleSeeder>>();
        var db = services.GetRequiredService<IdentityDbContext>();
        var uow = services.GetRequiredService<IIdentityUnitOfWork>();
        var setter = services.GetRequiredService<IRequestContextSetter>();

        if (await db.Roles.IgnoreQueryFilters().AnyAsync(ct))
        {
            logger.LogInformation("IdentityRoleSeeder skipped â€” data already exists");
            return;
        }

        logger.LogInformation("IdentityRoleSeeder started");

        var conn = db.Database.GetDbConnection();
        var tenant = await conn.QueryFirstOrDefaultAsync<TenantRow>(GetFirstTenantSql);

        if (tenant is null)
        {
            logger.LogWarning("IdentityRoleSeeder skipped â€” no tenant found in atlas_platform.tenants");
            return;
        }

        var policy = services.GetRequiredService<IPermissionPolicy>();

        IEnumerable<string> memberPermissions =
        [
            StaffModulePermissions.StaffMember.Read,
            StaffModulePermissions.StaffMember.Create,
            StaffModulePermissions.StaffMember.Update,
            StaffModulePermissions.StaffMember.Deactivate,
        ];

        var roleRepository = services.GetRequiredService<IRoleRepository>();

        var root = Role.Create(
            tenant.TenantId,
            "root",
            policy.AllIncludingSystem,
            policy.AllIncludingSystem,
            isSystem: true,
            id: SystemRoleIds.Root
        );
        var admin = Role.Create(
            tenant.TenantId,
            "admin",
            policy.All,
            policy.All,
            isSystem: true,
            id: SystemRoleIds.Admin
        );
        var member = Role.Create(
            tenant.TenantId,
            "member",
            memberPermissions,
            policy.All,
            isSystem: true,
            id: SystemRoleIds.Member
        );

        await roleRepository.AddAsync(root, ct);
        await roleRepository.AddAsync(admin, ct);
        await roleRepository.AddAsync(member, ct);

        setter.Set(tenant.TenantId, tenant.TenantName, SystemIdentity.UserId, SystemIdentity.Email);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("IdentityRoleSeeder completed:");
        logger.LogInformation("  Tenant : {Name} ({Id})", tenant.TenantName, tenant.TenantId);
        logger.LogInformation("  Roles  : root, admin, member");
    }
}
