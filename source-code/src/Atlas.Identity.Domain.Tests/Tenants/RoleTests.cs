using Atlas.Identity.Contracts.Permissions;
using Atlas.Identity.Domain.Tenants._Roles;
using Atlas.Identity.Domain.Tenants._Roles.Exceptions;
using Atlas.Identity.Domain.Tenants.Events;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain.Permissions;
using FluentAssertions;
using StaffPermissions = Atlas.Staff.Contracts.Permissions;

namespace Atlas.Identity.Tests.Tenants;

public class RoleTests
{
    // ============================================================
    // SHARED PERMISSION SETS (test fixtures)
    // ============================================================

    private static readonly IReadOnlySet<string> AllCodes = new HashSet<string>
    {
        ModulePermissions.Roles.Read,
        ModulePermissions.Roles.Create,
        ModulePermissions.Roles.Update,
        ModulePermissions.Roles.Delete,
        ModulePermissions.Roles.Manage,
        ModulePermissions.Invitations.Read,
        ModulePermissions.Invitations.Create,
        ModulePermissions.Invitations.Update,
        ModulePermissions.Invitations.Delete,
        ModulePermissions.Invitations.Manage,
        StaffPermissions.ModulePermissions.Staff.Read,
        StaffPermissions.ModulePermissions.Staff.Create,
        StaffPermissions.ModulePermissions.Staff.Update,
        StaffPermissions.ModulePermissions.Staff.Deactivate,
        StaffPermissions.ModulePermissions.Staff.Manage,
    };

    private static readonly IReadOnlySet<string> AllIncludingSystemCodes = new HashSet<string>(AllCodes)
    {
        SystemPermissions.Root,
    };

    private static readonly Guid TenantId = Guid.NewGuid();

    // ============================================================
    // 1. CREATE
    // ============================================================

    [Fact]
    public void Create_ShouldCreateRole_WithCorrectData()
    {
        var role = Role.Create(TenantId, "support", [StaffPermissions.ModulePermissions.Staff.Read], AllCodes);

        role.TenantId.Should().Be(TenantId);
        role.Name.Should().Be("support");
        role.IsSystem.Should().BeFalse();
        role.IsActive.Should().BeTrue();
        role.Permissions.Select(p => p.Code).Should().Contain(StaffPermissions.ModulePermissions.Staff.Read);
    }

    [Fact]
    public void Create_ShouldEmitRoleCreatedEvent()
    {
        var role = Role.Create(TenantId, "support", [], AllCodes);

        var evt = role.DomainEvents.OfType<RoleCreatedDomainEvent>().Single();
        evt.TenantId.Should().Be(TenantId);
        evt.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void Create_ShouldThrow_WhenNameIsTooShort()
    {
        var act = () => Role.Create(TenantId, "ab", [], AllCodes);

        act.Should().Throw<InvalidRoleNameException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenNameIsTooLong()
    {
        var act = () => Role.Create(TenantId, "toolongname", [], AllCodes);

        act.Should().Throw<InvalidRoleNameException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenPermissionCodeIsInvalid()
    {
        var act = () => Role.Create(TenantId, "custom", ["unknown.permission"], AllCodes);

        act.Should().Throw<RoleWithInvalidPermissionException>();
    }

    [Fact]
    public void Create_ShouldUseProvidedId_WhenIdIsSupplied()
    {
        var fixedId = Guid.NewGuid();

        var role = Role.Create(TenantId, "support", [], AllCodes, id: fixedId);

        role.Id.Should().Be(fixedId);
    }

    [Fact]
    public void Create_ShouldMarkAsSystem_WhenIsSystemIsTrue()
    {
        var role = Role.Create(TenantId, "admin", AllCodes, AllCodes, isSystem: true);

        role.IsSystem.Should().BeTrue();
    }

    // ============================================================
    // 2. DEACTIVATE (soft delete)
    // ============================================================

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var role = Role.Create(TenantId, "support", [], AllCodes);

        role.Deactivate();

        role.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldEmitRoleDeactivatedEvent()
    {
        var role = Role.Create(TenantId, "support", [], AllCodes);
        role.ClearDomainEvents();

        role.Deactivate();

        var evt = role.DomainEvents.OfType<RoleDeactivatedDomainEvent>().Single();
        evt.TenantId.Should().Be(TenantId);
        evt.RoleId.Should().Be(role.Id);
    }

    // ============================================================
    // 3. DELETE (hard delete marker)
    // ============================================================

    [Fact]
    public void Delete_ShouldEmitRoleDeletedEvent()
    {
        var role = Role.Create(TenantId, "support", [], AllCodes);
        role.ClearDomainEvents();

        role.Delete();

        var evt = role.DomainEvents.OfType<RoleDeletedDomainEvent>().Single();
        evt.TenantId.Should().Be(TenantId);
        evt.RoleId.Should().Be(role.Id);
    }

    // ============================================================
    // 4. RENAME
    // ============================================================

    [Fact]
    public void Rename_ShouldUpdateName_WhenNameIsValid()
    {
        var role = Role.Create(TenantId, "custom", [], AllCodes);

        role.Rename("renamed");

        role.Name.Should().Be("renamed");
    }

    [Fact]
    public void Rename_ShouldThrow_WhenNameIsTooShort()
    {
        var role = Role.Create(TenantId, "custom", [], AllCodes);

        var act = () => role.Rename("ab");

        act.Should().Throw<InvalidRoleNameException>();
    }

    [Fact]
    public void Rename_ShouldThrow_WhenNameIsTooLong()
    {
        var role = Role.Create(TenantId, "custom", [], AllCodes);

        var act = () => role.Rename("toolongname");

        act.Should().Throw<InvalidRoleNameException>();
    }

    // ============================================================
    // 5. UPDATE PERMISSIONS
    // ============================================================

    [Fact]
    public void UpdatePermissions_ShouldReplacePermissions_WhenRoleIsCustom()
    {
        var role = Role.Create(TenantId, "custom", [StaffPermissions.ModulePermissions.Staff.Read], AllCodes);
        role.ClearDomainEvents();

        role.UpdatePermissions(
            [StaffPermissions.ModulePermissions.Staff.Read, StaffPermissions.ModulePermissions.Staff.Update],
            AllCodes
        );

        role.Permissions.Select(p => p.Code)
            .Should()
            .BeEquivalentTo([
                StaffPermissions.ModulePermissions.Staff.Read,
                StaffPermissions.ModulePermissions.Staff.Update,
            ]);
    }

    [Fact]
    public void UpdatePermissions_ShouldEmitRoleUpdatedEvent()
    {
        var role = Role.Create(TenantId, "custom", [], AllCodes);
        role.ClearDomainEvents();

        role.UpdatePermissions([StaffPermissions.ModulePermissions.Staff.Read], AllCodes);

        var evt = role.DomainEvents.OfType<RoleUpdatedDomainEvent>().Single();
        evt.TenantId.Should().Be(TenantId);
        evt.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void UpdatePermissions_ShouldThrow_WhenRoleIsSystem()
    {
        var role = Role.Create(TenantId, "admin", AllCodes, AllCodes, isSystem: true);

        var act = () => role.UpdatePermissions([StaffPermissions.ModulePermissions.Staff.Read], AllCodes);

        act.Should().Throw<SystemRoleCannotBeModifiedException>();
    }

    [Fact]
    public void UpdatePermissions_ShouldThrow_WhenPermissionCodeIsInvalid()
    {
        var role = Role.Create(TenantId, "custom", [], AllCodes);

        var act = () => role.UpdatePermissions(["invalid.code"], AllCodes);

        act.Should().Throw<RoleWithInvalidPermissionException>();
    }
}
