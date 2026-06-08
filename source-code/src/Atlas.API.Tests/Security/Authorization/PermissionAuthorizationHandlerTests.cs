using System.Security.Claims;
using Atlas.BuildingBlocks.AspNetCore.Security;
using Atlas.BuildingBlocks.AspNetCore.Security.Authorization;
using Atlas.Identity.Contracts.Permissions;
using Atlas.SharedKernel.Application;
using Atlas.Staff.Contracts.Permissions;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;

namespace Atlas.API.Tests.Security.Authorization;

public class PermissionAuthorizationHandlerTests
{
    private static readonly PermissionAuthorizationHandler Handler = new();

    [Fact]
    public async Task HandleRequirementAsync_ShouldAuthorize_WhenUserHasDirectPermission()
    {
        var code    = IdentityModulePermissions.Roles.Read.Code;
        var context = CreateContext(code, code);

        await Handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldAuthorize_WhenUserHasManagerPermissionInSameGroup()
    {
        var required = IdentityModulePermissions.Roles.Read.Code;
        var granted  = IdentityModulePermissions.Roles.Manage.Code;
        var context  = CreateContext(required, granted);

        await Handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldNotAuthorize_AcrossModulesWithSameShortGroup()
    {
        var required = StaffModulePermissions.Audit.Read;
        var granted  = IdentityModulePermissions.Audit.Manage.Code;
        var context  = CreateContext(required, granted);

        await Handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldAuthorize_WhenUserHasRootPermission()
    {
        var context = CreateContext(IdentityModulePermissions.Roles.Read.Code, SystemPermissions.Root);

        await Handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldNotAuthorize_WhenNoPermissions()
    {
        var context = CreateContext(IdentityModulePermissions.Roles.Read.Code);

        await Handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    private static AuthorizationHandlerContext CreateContext(string requiredPermission, params string[] grantedPermissions)
    {
        var claims = grantedPermissions
            .Select(p => new Claim(AtlasClaims.Permission, p))
            .ToList();

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        return new AuthorizationHandlerContext(
            [new PermissionRequirement(requiredPermission)],
            principal,
            resource: null);
    }
}
