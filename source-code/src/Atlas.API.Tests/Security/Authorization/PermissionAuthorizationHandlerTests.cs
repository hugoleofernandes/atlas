using System.Security.Claims;
using Atlas.BuildingBlocks.AspNetCore.Security;
using Atlas.BuildingBlocks.AspNetCore.Security.Authorization;
using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Contracts.Permissions;
using Atlas.Platform.Contracts.Permissions;
using Atlas.Staff.Contracts.Permissions;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;

namespace Atlas.API.Tests.Security.Authorization;

public class PermissionAuthorizationHandlerTests
{
    private static readonly IPermissionPolicy Policy = new PermissionPolicyService(
    [
        new IdentityModulePermissions(),
        new StaffModulePermissions(),
        new PlatformModulePermissions(),
    ]);

    [Fact]
    public async Task HandleRequirementAsync_ShouldAuthorize_WhenUserHasDirectPermission()
    {
        var context = CreateContext(
            IdentityModulePermissions.Roles.Read.Code,
            IdentityModulePermissions.Roles.Read.Code);

        var handler = new PermissionAuthorizationHandler(Policy);
        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldAuthorize_WhenUserHasManagerPermissionInSameGroup()
    {
        var context = CreateContext(
            IdentityModulePermissions.Roles.Read.Code,
            IdentityModulePermissions.Roles.Manage.Code);

        var handler = new PermissionAuthorizationHandler(Policy);
        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldNotAuthorize_AcrossModulesWithSameShortGroup()
    {
        var context = CreateContext(
            StaffModulePermissions.Audit.Read,
            IdentityModulePermissions.Audit.Manage.Code);

        var handler = new PermissionAuthorizationHandler(Policy);
        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    private static AuthorizationHandlerContext CreateContext(string requiredPermission, params string[] grantedPermissions)
    {
        var claims = grantedPermissions
            .Select(permission => new Claim(AtlasClaims.Permission, permission))
            .ToList();

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        return new AuthorizationHandlerContext(
            [new PermissionRequirement(requiredPermission)],
            principal,
            resource: null);
    }
}
