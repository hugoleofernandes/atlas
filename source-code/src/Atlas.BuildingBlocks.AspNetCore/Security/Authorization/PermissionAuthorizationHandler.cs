using Atlas.BuildingBlocks.AspNetCore.Security;
using Microsoft.AspNetCore.Authorization;

namespace Atlas.BuildingBlocks.AspNetCore.Security.Authorization;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission) => Permission = permission;
}

/// <summary>
/// Checks whether the authenticated user holds the required permission claim.
/// Permission claims are injected by UserBootstrapMiddleware at login time.
///
/// Authorization logic is pure business rules derived from the permission code format
/// — no DB, no cache, no network. Synchronous and zero-dependency.
///
/// Rules:
///   1. system.root           → universal access (master key)
///   2. exact claim match     → direct access
///   3. {module}.{group}.manage satisfies any {module}.{group}.{verb} → manage coverage
///
/// The format contract {module}.{group}.{verb} is validated at declaration time
/// in PermissionDefinition and documented in CLAUDE.md.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var grantedCodes = context.User
            .FindAll(AtlasClaims.Permission)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.Ordinal);

        if (grantedCodes.Count == 0)
            return Task.CompletedTask;

        // 1. system.root — master key, satisfies any permission
        if (grantedCodes.Contains(AtlasClaims.RootPermission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 2. Exact claim match
        if (grantedCodes.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 3. Manage coverage: {module}.{group}.manage satisfies any {module}.{group}.{verb}
        //    The code format <module>.<group>.<verb> is the contract — "manage" is the verb
        //    that grants all actions within the same module + group.
        var parts = requirement.Permission.Split('.');
        if (parts.Length == 3)
        {
            var manageCode = $"{parts[0]}.{parts[1]}.manage";
            if (grantedCodes.Contains(manageCode))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
