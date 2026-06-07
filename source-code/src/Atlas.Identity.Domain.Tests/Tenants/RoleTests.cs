using Atlas.Identity.Domain.Tenants._Roles;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.Identity.Domain.Tenants._Roles.Exceptions;
using Atlas.Identity.Domain.Tenants.Events;
using Atlas.Identity.Contracts.Permissions;
using Atlas.Staff.Contracts.Permissions;
using FluentAssertions;

namespace Atlas.Identity.Tests.Tenants;

public class RoleTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldCreateRole_WithCorrectData()
    {
        var role = Role.Create(TenantId, "support", PermissionFixtures.Resolve(StaffModulePermissions.StaffMember.Read));

        role.TenantId.Should().Be(TenantId);
        role.Name.Should().Be("support");
        role.IsSystem.Should().BeFalse();
        role.IsActive.Should().BeTrue();
        role.Permissions.Select(p => p.Code).Should().Contain(StaffModulePermissions.StaffMember.Read);
    }

    [Fact]
    public void Create_ShouldStoreManagerMetadata_WhenPermissionIsManage()
    {
        var role = Role.Create(TenantId, "custom", PermissionFixtures.Resolve(IdentityModulePermissions.Roles.Manage));

        var permission = role.Permissions.Single();
        permission.Code.Should().Be(IdentityModulePermissions.Roles.Manage.Code);
        permission.Group.Should().Be("roles");
        permission.IsManager.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldEmitRoleCreatedEvent()
    {
        var role = Role.Create(TenantId, "support", []);

        var evt = role.DomainEvents.OfType<RoleCreatedDomainEvent>().Single();
        evt.TenantId.Should().Be(TenantId);
        evt.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void Create_ShouldThrow_WhenNameIsTooShort()
    {
        var act = () => Role.Create(TenantId, "ab", []);

        act.Should().Throw<InvalidRoleNameException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenNameIsTooLong()
    {
        var act = () => Role.Create(TenantId, "toolongname", []);

        act.Should().Throw<InvalidRoleNameException>();
    }

    [Fact]
    public void Create_ShouldUseProvidedId_WhenIdIsSupplied()
    {
        var fixedId = Guid.NewGuid();

        var role = Role.Create(TenantId, "support", [], id: fixedId);

        role.Id.Should().Be(fixedId);
    }

    [Fact]
    public void Create_ShouldMarkAsSystem_WhenIsSystemIsTrue()
    {
        var role = Role.Create(TenantId, "admin", PermissionFixtures.Resolve(PermissionFixtures.AllCodes.ToArray()), isSystem: true);

        role.IsSystem.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldNormalizeDuplicatePermissions_ByCode()
    {
        var duplicated = new[]
        {
            Permission.Of(IdentityModulePermissions.Roles.Read.Code, "roles", false),
            Permission.Of(IdentityModulePermissions.Roles.Read.Code, "roles", false),
        };

        var role = Role.Create(TenantId, "custom", duplicated);

        role.Permissions.Should().HaveCount(1);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var role = Role.Create(TenantId, "support", []);

        role.Deactivate();

        role.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldEmitRoleDeactivatedEvent()
    {
        var role = Role.Create(TenantId, "support", []);
        role.ClearDomainEvents();

        role.Deactivate();

        var evt = role.DomainEvents.OfType<RoleDeactivatedDomainEvent>().Single();
        evt.TenantId.Should().Be(TenantId);
        evt.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void Delete_ShouldEmitRoleDeletedEvent()
    {
        var role = Role.Create(TenantId, "support", []);
        role.ClearDomainEvents();

        role.Delete();

        var evt = role.DomainEvents.OfType<RoleDeletedDomainEvent>().Single();
        evt.TenantId.Should().Be(TenantId);
        evt.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void Rename_ShouldUpdateName_WhenNameIsValid()
    {
        var role = Role.Create(TenantId, "custom", []);

        role.Rename("renamed");

        role.Name.Should().Be("renamed");
    }

    [Fact]
    public void Rename_ShouldThrow_WhenNameIsTooShort()
    {
        var role = Role.Create(TenantId, "custom", []);

        var act = () => role.Rename("ab");

        act.Should().Throw<InvalidRoleNameException>();
    }

    [Fact]
    public void Rename_ShouldThrow_WhenNameIsTooLong()
    {
        var role = Role.Create(TenantId, "custom", []);

        var act = () => role.Rename("toolongname");

        act.Should().Throw<InvalidRoleNameException>();
    }

    [Fact]
    public void UpdatePermissions_ShouldReplacePermissions_WhenRoleIsCustom()
    {
        var role = Role.Create(TenantId, "custom", PermissionFixtures.Resolve(StaffModulePermissions.StaffMember.Read));
        role.ClearDomainEvents();

        role.UpdatePermissions(PermissionFixtures.Resolve(
            StaffModulePermissions.StaffMember.Read,
            StaffModulePermissions.StaffMember.Update));

        role.Permissions.Select(p => p.Code)
            .Should()
            .BeEquivalentTo([StaffModulePermissions.StaffMember.Read, StaffModulePermissions.StaffMember.Update]);
    }

    [Fact]
    public void UpdatePermissions_ShouldEmitRoleUpdatedEvent()
    {
        var role = Role.Create(TenantId, "custom", []);
        role.ClearDomainEvents();

        role.UpdatePermissions(PermissionFixtures.Resolve(StaffModulePermissions.StaffMember.Read));

        var evt = role.DomainEvents.OfType<RoleUpdatedDomainEvent>().Single();
        evt.TenantId.Should().Be(TenantId);
        evt.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void UpdatePermissions_ShouldThrow_WhenRoleIsSystem()
    {
        var role = Role.Create(
            TenantId,
            "admin",
            PermissionFixtures.Resolve(PermissionFixtures.AllCodes.ToArray()),
            isSystem: true);

        var act = () => role.UpdatePermissions(PermissionFixtures.Resolve(StaffModulePermissions.StaffMember.Read));

        act.Should().Throw<SystemRoleCannotBeModifiedException>();
    }
}
