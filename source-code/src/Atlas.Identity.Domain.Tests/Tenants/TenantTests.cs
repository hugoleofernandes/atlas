using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.Entities.Tenants.Events;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.ValueObjects;
using FluentAssertions;

namespace Atlas.Identity.Tests.Tenants;

public class TenantTests
{
    // ============================================================
    // HELPERS
    // ============================================================

    /// <summary>
    /// Creates a tenant with default system roles seeded.
    /// Returns the tenant and the Id of the "admin" role.
    /// </summary>
    private static (Tenant tenant, Guid adminRoleId, Guid memberRoleId) CreateTenantWithRoles(string name = "test")
    {
        var tenant = new Tenant(name);
        tenant.SeedDefaultRoles();
        tenant.ClearDomainEvents();
        var adminRoleId = tenant.Roles.Single(r => r.Name == "admin").Id;
        var memberRoleId = tenant.Roles.Single(r => r.Name == "member").Id;
        return (tenant, adminRoleId, memberRoleId);
    }

    /// <summary>
    /// Forces an invitation's ExpiresAt into the past via reflection.
    /// Avoids Thread.Sleep — deterministic and fast in any environment.
    /// </summary>
    private static void ForceExpire(Invitation invitation)
    {
        typeof(Invitation)
            .GetProperty(nameof(Invitation.ExpiresAt))!
            .SetValue(invitation, DateTime.UtcNow.AddSeconds(-1));
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
    public void SeedDefaultRoles_ShouldCreateAdminMemberAndViewerRoles()
    {
        var tenant = new Tenant("test");

        tenant.SeedDefaultRoles();

        tenant.Roles.Should().HaveCount(3);
        tenant.Roles.Should().Contain(r => r.Name == "admin" && r.IsSystem);
        tenant.Roles.Should().Contain(r => r.Name == "member" && r.IsSystem);
        tenant.Roles.Should().Contain(r => r.Name == "viewer" && r.IsSystem);
    }

    [Fact]
    public void SeedDefaultRoles_AdminShouldHaveAllPermissions()
    {
        var tenant = new Tenant("test");

        tenant.SeedDefaultRoles();

        var admin = tenant.Roles.Single(r => r.Name == "admin");
        admin.Permissions.Select(p => p.Code).Should()
            .BeEquivalentTo(PermissionCatalog.All);
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
    // 4. INVITE USER
    // ============================================================

    [Fact]
    public void InviteUser_ShouldThrow_WhenTenantIsInactive()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();
        tenant.Deactivate();

        var act = () => tenant.InviteUser(
            Email.Create("user@test.com"),
            adminRoleId,
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        act.Should().Throw<TenantInactiveException>();
    }

    [Fact]
    public void InviteUser_ShouldThrow_WhenRoleDoesNotExist()
    {
        var (tenant, _, _) = CreateTenantWithRoles();
        var unknownRoleId = Guid.NewGuid();

        var act = () => tenant.InviteUser(
            Email.Create("user@test.com"),
            unknownRoleId,
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        act.Should().Throw<RoleNotFoundException>();
    }

    [Fact]
    public void InviteUser_ShouldThrow_WhenUserAlreadyExists()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();

        tenant.InviteUser(Email.Create("user@test.com"), adminRoleId, InvitationTtl.Create(TimeSpan.FromHours(1)));
        tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("user@test.com"));

        var act = () => tenant.InviteUser(
            Email.Create("user@test.com"),
            adminRoleId,
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        act.Should().Throw<UserAlreadyExistsException>();
    }

    [Fact]
    public void InviteUser_ShouldThrow_WhenActiveInvitationAlreadyExists()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();

        tenant.InviteUser(Email.Create("user@test.com"), adminRoleId, InvitationTtl.Create(TimeSpan.FromHours(1)));

        var act = () => tenant.InviteUser(
            Email.Create("user@test.com"),
            adminRoleId,
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        act.Should().Throw<DuplicateInvitationException>();
    }

    [Fact]
    public void InviteUser_ShouldSucceed_WhenPreviousInvitationIsExpired()
    {
        var (tenant, adminRoleId, memberRoleId) = CreateTenantWithRoles();

        tenant.InviteUser(Email.Create("user@test.com"), adminRoleId, InvitationTtl.Create(TimeSpan.FromHours(1)));
        ForceExpire(tenant.Invitations.Single());

        var act = () => tenant.InviteUser(
            Email.Create("user@test.com"),
            memberRoleId,
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        act.Should().NotThrow();
        tenant.Invitations.Should().HaveCount(2);
    }

    [Fact]
    public void InviteUser_ShouldEmitUserInvitedEvent_WhenValid()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();

        tenant.InviteUser(Email.Create("user@test.com"), adminRoleId, InvitationTtl.Create(TimeSpan.FromHours(1)));

        tenant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserInvitedDomainEvent>();
    }

    [Fact]
    public void InviteUser_ShouldEmitEventWithCorrectData_WhenValid()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();

        tenant.InviteUser(Email.Create("user@test.com"), adminRoleId, InvitationTtl.Create(TimeSpan.FromHours(1)));

        var evt = tenant.DomainEvents.OfType<UserInvitedDomainEvent>().Single();
        evt.TenantId.Should().Be(tenant.Id);
        evt.Email.Should().Be("user@test.com");
    }

    [Fact]
    public void InviteUser_ShouldAddInvitation_WhenValid()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();

        tenant.InviteUser(Email.Create("user@test.com"), adminRoleId, InvitationTtl.Create(TimeSpan.FromHours(1)));

        tenant.Invitations.Should().ContainSingle(i => i.Email.Value == "user@test.com");
    }

    // ============================================================
    // 5. ADD CUSTOM ROLE
    // ============================================================

    [Fact]
    public void AddCustomRole_ShouldAddRole_WhenNameIsUnique()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var role = tenant.AddCustomRole("supervisor", [PermissionCatalog.Staff.Read, PermissionCatalog.Staff.Update]);

        tenant.Roles.Should().Contain(r => r.Name == "supervisor");
        role.IsSystem.Should().BeFalse();
        role.Permissions.Select(p => p.Code).Should().Contain([PermissionCatalog.Staff.Read, PermissionCatalog.Staff.Update]);
    }

    [Fact]
    public void AddCustomRole_ShouldThrow_WhenNameAlreadyExists()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var act = () => tenant.AddCustomRole("admin", [PermissionCatalog.Staff.Read]);

        act.Should().Throw<RoleAlreadyExistsException>();
    }

    [Fact]
    public void AddCustomRole_ShouldThrow_WhenPermissionCodeIsInvalid()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var act = () => tenant.AddCustomRole("custom", ["unknown.permission"]);

        act.Should().Throw<InvalidPermissionException>();
    }

    // ============================================================
    // 6. UPDATE ROLE PERMISSIONS
    // ============================================================

    [Fact]
    public void UpdateRolePermissions_ShouldUpdatePermissions_WhenRoleIsCustom()
    {
        var (tenant, _, _) = CreateTenantWithRoles();
        var custom = tenant.AddCustomRole("custom", [PermissionCatalog.Staff.Read]);
        tenant.ClearDomainEvents();

        tenant.UpdateRolePermissions(custom.Id, [PermissionCatalog.Staff.Read, PermissionCatalog.Staff.Update]);

        custom.Permissions.Select(p => p.Code).Should()
            .BeEquivalentTo([PermissionCatalog.Staff.Read, PermissionCatalog.Staff.Update]);
        tenant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TenantRoleUpdatedDomainEvent>();
    }

    [Fact]
    public void UpdateRolePermissions_ShouldThrow_WhenRoleIsSystem()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();

        var act = () => tenant.UpdateRolePermissions(adminRoleId, [PermissionCatalog.Staff.Read]);

        act.Should().Throw<SystemRoleCannotBeModifiedException>();
    }

    [Fact]
    public void UpdateRolePermissions_ShouldThrow_WhenRoleDoesNotExist()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var act = () => tenant.UpdateRolePermissions(Guid.NewGuid(), [PermissionCatalog.Staff.Read]);

        act.Should().Throw<RoleNotFoundException>();
    }

    // ============================================================
    // 7. RESOLVE ACCESS
    // ============================================================

    [Fact]
    public void ResolveAccess_ShouldReturnExistingUser_WhenUserAlreadyExists()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();
        tenant.InviteUser(Email.Create("user@test.com"), adminRoleId, InvitationTtl.Create(TimeSpan.FromDays(1)));

        var user = tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("user@test.com"));
        tenant.ClearDomainEvents();

        var result = tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("user@test.com"));

        result.Should().Be(user);
        tenant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserAccessResolvedDomainEvent>();
    }

    [Fact]
    public void ResolveAccess_ShouldThrow_WhenTenantIsInactive()
    {
        var (tenant, _, _) = CreateTenantWithRoles();
        tenant.Deactivate();

        var act = () => tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("user@test.com"));

        act.Should().Throw<TenantInactiveException>();
    }

    [Fact]
    public void ResolveAccess_ShouldThrow_WhenInvitationDoesNotExist()
    {
        var (tenant, _, _) = CreateTenantWithRoles();

        var act = () => tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("missing@test.com"));

        act.Should().Throw<InvitationNotFoundException>();
    }

    [Fact]
    public void ResolveAccess_ShouldThrow_WhenInvitationIsExpired()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();
        tenant.InviteUser(Email.Create("user@test.com"), adminRoleId, InvitationTtl.Create(TimeSpan.FromHours(1)));
        ForceExpire(tenant.Invitations.Single());

        var act = () => tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("user@test.com"));

        act.Should().Throw<InvitationExpiredException>();
    }

    [Fact]
    public void ResolveAccess_ShouldThrow_WhenUserAlreadyExistsAfterInvitationUse()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();
        tenant.InviteUser(Email.Create("user@test.com"), adminRoleId, InvitationTtl.Create(TimeSpan.FromHours(1)));
        tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("user@test.com"));
        tenant.ClearDomainEvents();

        var act = () => tenant.ResolveAccess(ExternalId.Create("oid-2"), Email.Create("user@test.com"));

        act.Should().Throw<UserAlreadyExistsException>();
    }

    [Fact]
    public void ResolveAccess_ShouldThrow_WhenExistingUserHasDifferentExternalId()
    {
        // Security invariant: same email but different OID means a different identity
        // provider account is trying to claim the same user slot — must be rejected.
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();
        tenant.InviteUser(Email.Create("user@test.com"), adminRoleId, InvitationTtl.Create(TimeSpan.FromHours(1)));
        tenant.ResolveAccess(ExternalId.Create("oid-legitimate"), Email.Create("user@test.com"));
        tenant.ClearDomainEvents();

        var act = () => tenant.ResolveAccess(ExternalId.Create("oid-attacker"), Email.Create("user@test.com"));

        act.Should().Throw<UserAlreadyExistsException>();
    }

    [Fact]
    public void ResolveAccess_ShouldCreateUser_WhenInvitationIsValid()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();
        tenant.InviteUser(Email.Create("user@test.com"), adminRoleId, InvitationTtl.Create(TimeSpan.FromHours(1)));

        var user = tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("user@test.com"));

        user.Email.Value.Should().Be("user@test.com");
        user.TenantRoleId.Should().Be(adminRoleId);
        tenant.Users.Should().Contain(user);
    }

    [Fact]
    public void ResolveAccess_ShouldEmitEvents_WhenUserIsCreated()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();
        tenant.InviteUser(Email.Create("user@test.com"), adminRoleId, InvitationTtl.Create(TimeSpan.FromHours(1)));
        tenant.ClearDomainEvents();

        tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("user@test.com"));

        tenant.DomainEvents.Should().HaveCount(3);
        tenant.DomainEvents.Should().Contain(e => e is InvitationUsedDomainEvent);
        tenant.DomainEvents.Should().Contain(e => e is UserCreatedFromInvitationDomainEvent);
        tenant.DomainEvents.Should().Contain(e => e is UserAccessResolvedDomainEvent);
    }

    [Fact]
    public void ResolveAccess_ShouldEmitEventsWithCorrectData_WhenUserIsCreated()
    {
        var (tenant, adminRoleId, _) = CreateTenantWithRoles();
        tenant.InviteUser(Email.Create("user@test.com"), adminRoleId, InvitationTtl.Create(TimeSpan.FromHours(1)));
        tenant.ClearDomainEvents();

        var user = tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("user@test.com"));

        var createdEvt = tenant.DomainEvents.OfType<UserCreatedFromInvitationDomainEvent>().Single();
        createdEvt.TenantId.Should().Be(tenant.Id);
        createdEvt.UserId.Should().Be(user.Id);
        createdEvt.Email.Should().Be("user@test.com");
        createdEvt.Role.Should().Be("admin");

        var resolvedEvt = tenant.DomainEvents.OfType<UserAccessResolvedDomainEvent>().Single();
        resolvedEvt.TenantId.Should().Be(tenant.Id);
        resolvedEvt.UserId.Should().Be(user.Id);
    }
}
