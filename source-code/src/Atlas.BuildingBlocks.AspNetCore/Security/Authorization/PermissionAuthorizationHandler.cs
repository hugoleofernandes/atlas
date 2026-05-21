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
/// No database call per request — claims are in the cookie.
/// </summary>
public sealed class PermissionAuthorizationHandler
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

        var hasClaim = context.User.HasClaim(AtlasClaims.Permission, requirement.Permission);
        if (hasClaim)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
