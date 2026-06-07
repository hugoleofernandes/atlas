using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Permissions;
using Atlas.Identity.Domain.Tenants._Roles.Exceptions;
using Atlas.Identity.Contracts.Permissions;
using Atlas.Platform.Contracts.Permissions;
using Atlas.Staff.Contracts.Permissions;
using FluentAssertions;

namespace Atlas.API.Tests.Permissions;

public class PermissionResolutionTests
{
    private static readonly IPermissionPolicy Policy = new PermissionPolicyService(
    [
        new IdentityModulePermissions(),
        new StaffModulePermissions(),
        new PlatformModulePermissions(),
    ]);

    [Fact]
    public void Resolve_ShouldMaterializeManagerMetadata_ForManagePermission()
    {
        var permission = PermissionResolution.Resolve([IdentityModulePermissions.Roles.Manage], Policy).Single();

        permission.Code.Should().Be(IdentityModulePermissions.Roles.Manage.Code);
        permission.Group.Should().Be("roles");
        permission.IsManager.Should().BeTrue();
    }

    [Fact]
    public void Resolve_ShouldThrow_WhenPermissionCodeIsUnknown()
    {
        var act = () => PermissionResolution.Resolve(["identity.roles.unknown"], Policy);

        act.Should().Throw<RoleWithInvalidPermissionException>();
    }
}
