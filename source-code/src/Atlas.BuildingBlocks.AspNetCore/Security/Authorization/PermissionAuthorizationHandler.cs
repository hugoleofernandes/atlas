using Atlas.BuildingBlocks.Permissions;
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
/// No database call per request - claims are in the cookie and permission metadata is in-memory.
/// </summary>
public sealed class PermissionAuthorizationHandler(IPermissionPolicy permissionPolicy)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var isRoot = context.User.HasClaim(AtlasClaims.Permission, AtlasClaims.RootPermission);
        if (isRoot)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.User.HasClaim(AtlasClaims.Permission, requirement.Permission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (!permissionPolicy.DefinitionsByCode.TryGetValue(requirement.Permission, out var required))
            return Task.CompletedTask;

        var grantedCodes = context.User
            .FindAll(AtlasClaims.Permission)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal);

        foreach (var grantedCode in grantedCodes)
        {
            if (!permissionPolicy.DefinitionsByCode.TryGetValue(grantedCode, out var granted))
                continue;

            if (granted.IsManager
                && string.Equals(granted.ModuleName, required.ModuleName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(granted.Group, required.Group, StringComparison.Ordinal))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
