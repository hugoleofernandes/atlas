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

        if (context.User.HasClaim(AtlasClaims.Permission, requirement.Permission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // A user with {prefix}.manage satisfies any {prefix}.{verb} check.
        var lastDot = requirement.Permission.LastIndexOf('.');
        if (lastDot > 0)
        {
            var managePermission = requirement.Permission[..lastDot] + ".manage";
            if (context.User.HasClaim(AtlasClaims.Permission, managePermission))
                context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
