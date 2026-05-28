using Atlas.Staff.Domain.Permissions;
using FluentAssertions;
using Atlas.Identity.Domain.Tenants;
using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.Identity.Domain.Tenants.Events;
using Atlas.Identity.Domain.Tenants._Roles.Exceptions;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;

namespace Atlas.Identity.Tests.Tenants;

public class TenantTests
{
    // ============================================================
    // SHARED PERMISSION SETS (test fixtures)
    // ============================================================

    private static readonly IReadOnlySet<string> AllCodes = new HashSet<string>
    {
        IdentityModulePermissions.Tenant.Roles.Read,
        IdentityModulePermissions.Tenant.Roles.Create,
        IdentityModulePermissions.Tenant.Roles.Update,
        IdentityModulePermissions.Tenant.Roles.Delete,
        IdentityModulePermissions.Tenant.Roles.Manage,

        IdentityModulePermissions.Tenant.Invitations.Read,
        IdentityModulePermissions.Tenant.Invitations.Create,
        IdentityModulePermissions.Tenant.Invitations.Update,
        IdentityModulePermissions.Tenant.Invitations.Delete,
        IdentityModulePermissions.Tenant.Invitations.Manage,

        StaffPermissions.Read,
        StaffPermissions.Create,
        StaffPermissions.Update,
        StaffPermissions.Deactivate,
        StaffPermissions.Manage,
    };

    private static readonly IReadOnlySet<string> AllIncludingSystemCodes = new HashSet<string>(AllCodes)
    {
        SystemPermissions.Root,
    };

    private static readonly IEnumerable<string> DefaultMemberPermissions =
    [
        StaffPermissions.Read,
        StaffPermissions.Create,
        StaffPermissions.Update,
        StaffPermissions.Deactivate,
    ];

    // ============================================================
    // HELPERS
    // ============================================================

    private static (Tenant tenant, Guid adminRoleId, Guid memberRoleId) CreateTenantWithRoles(string name = "test")
    {
        var tenant = new Tenant(name);
        tenant.SeedDefaultRoles(AllCodes, AllIncludingSystemCodes, DefaultMemberPermissions);
        tenant.ClearDomainEvents();
        var adminRoleId = tenant.Roles.Single(r => r.Name == "admin").Id;
        var memberRoleId = tenant.Roles.Single(r => r.Name == "member").Id;
        return (tenant, adminRoleId, memberRoleId);
    }

    // ============================================================
    // 1. CONSTRUCTOR
    // ============================================================

    [Fact]
    public void Tenant_ShouldThrow_WhenNameIsMissing()
    {
        var act = () => new Tenant("");

        act.Should().Throw<TenantNameRequiredException>();
    }

    [Fact]
    public void Tenant_ShouldNormalizeName_WhenCreated()
    {
        var tenant = new Tenant("MyTenant");

        tenant.Name.Should().Be("mytenant");
    }

    // ============================================================
    // 2. SEED DEFAULT ROLES
    // ============================================================

    [Fact]
    public void SeedDefaultRoles_ShouldCreateRootAdminAndMemberRoles()
    {
        var tenant = new Tenant("test");

        tenant.SeedDefaultRoles(AllCodes, AllIncludingSystemCodes, DefaultMemberPermissions);

        tenant.Roles.Should().HaveCount(3);
        tenant.Roles.Should().Contain(r => r.Name == "root"   && r.IsSystem);
        tenant.Roles.Should().Contain(r => r.Name == "admin"  && r.IsSystem);
        tenant.Roles.Should().Contain(r => r.Name == "member" && r.IsSystem);
    }

    [Fact]
    public void SeedDefaultRoles_AdminShouldHaveAllPermissions()
    {
        var tenant = new Tenant("test");

        tenant.SeedDefaultRoles(AllCodes, AllIncludingSystemCodes, DefaultMemberPermissions);

        var admin = tenant.Roles.Single(r => r.Name == "admin");
        admin.Permissions.Select(p => p.Code).Should()
            .BeEquivalentTo(AllCodes);
    }

    // ============================================================
    // 3. LIFECYCLE: DEACTIVATE
    // ============================================================

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse_WhenTenantIsActive()
    {
        var tenant = new Tenant("test");

        tenant.Deactivate();

        tenant.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldEmitTenantDeactivatedEvent_WhenTenantIsActive()
    {
        var tenant = new Tenant("test");

        tenant.Deactivate();

        tenant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TenantDeactivatedDomainEvent>();
    }

    [Fact]
    public void Deactivate_ShouldEmitEventWithCorrectTenantId()
    {
        var tenant = new Tenant("test");

        tenant.Deactivate();

        var evt = tenant.DomainEvents.OfType<TenantDeactivatedDomainEvent>().Single();
        evt.TenantId.Should().Be(tenant.Id);
    }

    [Fact]
    public void Deactivate_ShouldDoNothing_WhenTenantIsAlreadyInactive()
    {
        var tenant = new Tenant("test");
        tenant.Deactivate();
        tenant.ClearDomainEvents();

        tenant.Deactivate();

        tenant.DomainEvents.Should().BeEmpty();
    }

    // ============================================================
    // 4. ENSURE ROLE EXISTS
    // ============================================================

    [Fact]
    public void EnsureRoleExists_ShouldThrow_WhenTenantIsInactive()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();
        tenant.Deactivate();

        var act = () => tenant.EnsureRoleExists(adminRoleId);

        act.Should().Throw<TenantInactiveException>();
    }

    [Fact]
    public void EnsureRoleExists_ShouldThrow_WhenRoleDoesNotExist()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var act = () => tenant.EnsureRoleExists(Guid.NewGuid());

        act.Should().Throw<RoleNotFoundException>();
    }

    [Fact]
    public void EnsureRoleExists_ShouldNotThrow_WhenTenantIsActiveAndRoleExists()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();

        var act = () => tenant.EnsureRoleExists(adminRoleId);

        act.Should().NotThrow();
    }

    // ============================================================
    // 5. ADD CUSTOM ROLE
    // ============================================================

    [Fact]
    public void AddCustomRole_ShouldAddRole_WhenNameIsUnique()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var role = tenant.AddRole("supervisor", [StaffPermissions.Read, StaffPermissions.Update], AllCodes);

        tenant.Roles.Should().Contain(r => r.Name == "supervisor");
        role.IsSystem.Should().BeFalse();
        role.Permissions.Select(p => p.Code).Should().Contain([StaffPermissions.Read, StaffPermissions.Update]);
    }

    [Fact]
    public void AddCustomRole_ShouldBeActiveByDefault_WhenCreated()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var role = tenant.AddRole("support", [], AllCodes);

        role.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AddCustomRole_ShouldEmitRoleCreatedEvent_WhenValid()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var role = tenant.AddRole("support", [], AllCodes);

        tenant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RoleCreatedDomainEvent>();

        var evt = tenant.DomainEvents.OfType<RoleCreatedDomainEvent>().Single();
        evt.TenantId.Should().Be(tenant.Id);
        evt.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void AddCustomRole_ShouldThrow_WhenTenantIsInactive()
    {
        var (tenant, _, _) = CreateTenantWithRoles();
        tenant.Deactivate();

        var act = () => tenant.AddRole("support", [], AllCodes);

        act.Should().Throw<TenantInactiveException>();
    }

    [Fact]
    public void AddCustomRole_ShouldThrow_WhenNameAlreadyExists()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var act = () => tenant.AddRole("admin", [StaffPermissions.Read], AllCodes);

        act.Should().Throw<RoleAlreadyExistsException>();
    }

    [Fact]
    public void AddCustomRole_ShouldThrow_WhenNameIsTooShort()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var act = () => tenant.AddRole("ab", [], AllCodes);

        act.Should().Throw<InvalidRoleNameException>();
    }

    [Fact]
    public void AddCustomRole_ShouldThrow_WhenNameIsTooLong()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var act = () => tenant.AddRole("toolongname", [], AllCodes);

        act.Should().Throw<InvalidRoleNameException>();
    }

    [Fact]
    public void AddCustomRole_ShouldThrow_WhenPermissionCodeIsInvalid()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var act = () => tenant.AddRole("custom", ["unknown.permission"], AllCodes);

        act.Should().Throw<RoleWithInvalidPermissionException>();
    }

    // ============================================================
    // 6. REMOVE ROLE
    // ============================================================

    [Fact]
    public void RemoveRole_ShouldThrow_WhenTenantIsInactive()
    {
        var (tenant, _, _) = CreateTenantWithRoles();
        var role = tenant.AddRole("support", [], AllCodes);
        tenant.Deactivate();

        var act = () => tenant.RemoveRole(role.Id, false, false, false);

        act.Should().Throw<TenantInactiveException>();
    }

    [Fact]
    public void RemoveRole_ShouldThrow_WhenRoleDoesNotExist()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var act = () => tenant.RemoveRole(Guid.NewGuid(), false, false, false);

        act.Should().Throw<RoleNotFoundException>();
    }

    [Fact]
    public void RemoveRole_ShouldThrow_WhenRoleIsSystem()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();

        var act = () => tenant.RemoveRole(adminRoleId, false, false, false);

        act.Should().Throw<SystemRoleCannotBeModifiedException>();
    }

    [Fact]
    public void RemoveRole_ShouldThrow_WhenRoleHasActiveUsers()
    {
        var (tenant, _, _) = CreateTenantWithRoles();
        var role = tenant.AddRole("support", [], AllCodes);

        var act = () => tenant.RemoveRole(role.Id, hasActiveUsers: true, hasActiveInvitations: false, hasHistory: true);

        act.Should().Throw<RoleInUseByUsersException>();
    }

    [Fact]
    public void RemoveRole_ShouldThrow_WhenRoleHasActiveInvitations()
    {
        var (tenant, _, _) = CreateTenantWithRoles();
        var role = tenant.AddRole("support", [], AllCodes);

        var act = () => tenant.RemoveRole(role.Id, hasActiveUsers: false, hasActiveInvitations: true, hasHistory: true);

        act.Should().Throw<RoleInUseByInvitationsException>();
    }

    [Fact]
    public void RemoveRole_ShouldHardDelete_WhenRoleHasNoHistory()
    {
        var (tenant, _, _) = CreateTenantWithRoles();
        var role = tenant.AddRole("support", [], AllCodes);
        tenant.ClearDomainEvents();

        tenant.RemoveRole(role.Id, hasActiveUsers: false, hasActiveInvitations: false, hasHistory: false);

        tenant.Roles.Should().NotContain(r => r.Id == role.Id);
        tenant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RoleDeletedDomainEvent>();

        var evt = tenant.DomainEvents.OfType<RoleDeletedDomainEvent>().Single();
        evt.TenantId.Should().Be(tenant.Id);
        evt.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void RemoveRole_ShouldSoftDelete_WhenRoleHasHistory()
    {
        var (tenant, _, _) = CreateTenantWithRoles();
        var role = tenant.AddRole("support", [], AllCodes);
        tenant.ClearDomainEvents();

        tenant.RemoveRole(role.Id, hasActiveUsers: false, hasActiveInvitations: false, hasHistory: true);

        tenant.Roles.Should().Contain(r => r.Id == role.Id);
        role.IsActive.Should().BeFalse();
        tenant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RoleDeactivatedDomainEvent>();

        var evt = tenant.DomainEvents.OfType<RoleDeactivatedDomainEvent>().Single();
        evt.TenantId.Should().Be(tenant.Id);
        evt.RoleId.Should().Be(role.Id);
    }

    // ============================================================
    // 7. UPDATE ROLE
    // ============================================================

    [Fact]
    public void UpdateRole_ShouldUpdateNameAndPermissions_WhenRoleIsCustom()
    {
        var (tenant, _, _) = CreateTenantWithRoles();
        var custom = tenant.AddRole("custom", [StaffPermissions.Read], AllCodes);
        tenant.ClearDomainEvents();

        tenant.UpdateRole(custom.Id, "renamed", [StaffPermissions.Read, StaffPermissions.Update], AllCodes);

        custom.Name.Should().Be("renamed");
        custom.Permissions.Select(p => p.Code).Should()
            .BeEquivalentTo([StaffPermissions.Read, StaffPermissions.Update]);
        tenant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RoleUpdatedDomainEvent>();
    }

    [Fact]
    public void UpdateRole_ShouldThrow_WhenRoleIsSystem()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();

        var act = () => tenant.UpdateRole(adminRoleId, "newname", [StaffPermissions.Read], AllCodes);

        act.Should().Throw<SystemRoleCannotBeModifiedException>();
    }

    [Fact]
    public void UpdateRole_ShouldThrow_WhenRoleDoesNotExist()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var act = () => tenant.UpdateRole(Guid.NewGuid(), "any", [StaffPermissions.Read], AllCodes);

        act.Should().Throw<RoleNotFoundException>();
    }

    [Fact]
    public void UpdateRole_ShouldThrow_WhenNameAlreadyExistsInAnotherRole()
    {
        var (tenant, _, _) = CreateTenantWithRoles();
        tenant.AddRole("roleone", [StaffPermissions.Read], AllCodes);
        var roleTwo = tenant.AddRole("roletwo", [StaffPermissions.Read], AllCodes);

        var act = () => tenant.UpdateRole(roleTwo.Id, "roleone", [StaffPermissions.Read], AllCodes);

        act.Should().Throw<RoleAlreadyExistsException>();
    }

    [Fact]
    public void UpdateRole_ShouldAllowKeepingSameName()
    {
        var (tenant, _, _) = CreateTenantWithRoles();
        var custom = tenant.AddRole("custom", [StaffPermissions.Read], AllCodes);
        tenant.ClearDomainEvents();

        var act = () => tenant.UpdateRole(custom.Id, "custom", [StaffPermissions.Update], AllCodes);

        act.Should().NotThrow();
        custom.Permissions.Select(p => p.Code).Should().BeEquivalentTo([StaffPermissions.Update]);
    }
}
