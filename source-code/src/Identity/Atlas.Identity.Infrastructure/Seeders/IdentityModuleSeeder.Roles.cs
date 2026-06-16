using Atlas.Identity.Domain.Roles;
using Atlas.Identity.Domain.Tenants._Roles;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Infrastructure.Seeders;

public sealed partial class IdentityModuleSeeder
{
    private async Task SeedRolesAsync(CancellationToken ct)
    {
        if (await db.Roles.AnyAsync(ct))
        {
            logger.LogInformation("IdentityRoleSeeder skipped - data already exists");
            return;
        }

        logger.LogInformation("IdentityRoleSeeder started");

        var tenantId =
            requestContext.TenantId ?? throw new InvalidOperationException("TenantId must be set in request context");

        var allPermissions = await catalogCache.GetAllActiveAsync(ct);

        // root   → system.root only (IsRoot=true) — the auth handler grants everything from this single permission
        // admin  → all manage permissions (IsManager=true) — covers every verb within each resource group
        // member → read-only across all resources
        var rootPerms = allPermissions.Where(p => p.IsRoot).Select(p => RolePermission.Of(p.Id));
        var adminPerms = allPermissions.Where(p => p.IsManager).Select(p => RolePermission.Of(p.Id));
        var memberPerms = allPermissions
            .Where(p => p.Code.EndsWith(".read", StringComparison.Ordinal))
            .Select(p => RolePermission.Of(p.Id));

        var root = Role.Create(tenantId, "root", rootPerms, isSystem: true, id: SystemRoleIds.Root);
        var admin = Role.Create(tenantId, "admin", adminPerms, isSystem: true, id: SystemRoleIds.Admin);
        var member = Role.Create(tenantId, "member", memberPerms, isSystem: true, id: SystemRoleIds.Member);

        await roleRepository.AddAsync(root, ct);
        await roleRepository.AddAsync(admin, ct);
        await roleRepository.AddAsync(member, ct);

        logger.LogInformation("  Created root   ({Count} permissions)", root.Permissions.Count);
        logger.LogInformation("  Created admin  ({Count} permissions)", admin.Permissions.Count);
        logger.LogInformation("  Created member ({Count} permissions)", member.Permissions.Count);

        await uow.SaveChangesAsync(ct);

        // Assign root role to the root user (created during bootstrap) — lookup by deterministic ID
        var rootUser = await db.Users.FirstOrDefaultAsync(u => u.Id == BootstrapIdentity.RootUser.Id, ct);

        if (rootUser is not null && rootUser.RoleId == Guid.Empty)
        {
            rootUser.ChangeRole(root.Id);
            await uow.SaveChangesAsync(ct);
            logger.LogInformation("  Root role assigned to {Email}", BootstrapIdentity.RootUser.Email);
        }

        logger.LogInformation("IdentityRoleSeeder completed — roles: root, admin, member");
    }
}
