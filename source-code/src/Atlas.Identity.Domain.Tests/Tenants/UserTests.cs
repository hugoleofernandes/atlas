using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Tenants;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.Identity.Domain.Users;
using Atlas.Identity.Domain.Users.Events;
using Atlas.Identity.Domain.Users.Exceptions;
using Atlas.Staff.Domain.Permissions;
using FluentAssertions;

namespace Atlas.Identity.Tests.Tenants;

public class UserTests
{
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

    private static (Tenant tenant, Guid adminRoleId) CreateTenantWithRoles()
    {
        var tenant = new Tenant("test");
        tenant.SeedDefaultRoles(AllCodes, AllIncludingSystemCodes, DefaultMemberPermissions);
        tenant.ClearDomainEvents();
        return (tenant, tenant.Roles.Single(r => r.Name == "admin").Id);
    }

    private static Invitation CreateUsedInvitation(Tenant tenant, Guid roleId, string email = "user@test.com")
    {
        var invitation = Invitation.Create(
            tenant.Id, Email.Create(email), roleId, InvitationTtl.Create(TimeSpan.FromHours(1)));
        invitation.Use();
        invitation.ClearDomainEvents();
        return invitation;
    }

    // ============================================================
    // CreateFromInvitation
    // ============================================================

    [Fact]
    public void CreateFromInvitation_ShouldCreateUserWithCorrectData()
    {
        var (tenant, adminRoleId) = CreateTenantWithRoles();
        var invitation = CreateUsedInvitation(tenant, adminRoleId);

        var user = User.CreateFromInvitation(invitation, ExternalId.Create("oid-1"), "admin");

        user.TenantId.Should().Be(tenant.Id);
        user.Email.Value.Should().Be("user@test.com");
        user.RoleId.Should().Be(adminRoleId);
        user.ExternalId.Value.Should().Be("oid-1");
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateFromInvitation_ShouldEmitUserCreatedEvent()
    {
        var (tenant, adminRoleId) = CreateTenantWithRoles();
        var invitation = CreateUsedInvitation(tenant, adminRoleId);

        var user = User.CreateFromInvitation(invitation, ExternalId.Create("oid-1"), "admin");

        var evt = user.DomainEvents.OfType<UserCreatedFromInvitationDomainEvent>().Single();
        evt.TenantId.Should().Be(tenant.Id);
        evt.UserId.Should().Be(user.Id);
        evt.Email.Should().Be("user@test.com");
        evt.Role.Should().Be("admin");
    }

    [Fact]
    public void CreateFromInvitation_ShouldEmitUserAccessResolvedEvent()
    {
        var (tenant, adminRoleId) = CreateTenantWithRoles();
        var invitation = CreateUsedInvitation(tenant, adminRoleId);

        var user = User.CreateFromInvitation(invitation, ExternalId.Create("oid-1"), "admin");

        var evt = user.DomainEvents.OfType<UserAccessResolvedDomainEvent>().Single();
        evt.TenantId.Should().Be(tenant.Id);
        evt.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void CreateFromInvitation_ShouldEmitBothEventsInOrder()
    {
        var (tenant, adminRoleId) = CreateTenantWithRoles();
        var invitation = CreateUsedInvitation(tenant, adminRoleId);

        var user = User.CreateFromInvitation(invitation, ExternalId.Create("oid-1"), "admin");

        user.DomainEvents.Should().HaveCount(2);
        user.DomainEvents.ElementAt(0).Should().BeOfType<UserCreatedFromInvitationDomainEvent>();
        user.DomainEvents.ElementAt(1).Should().BeOfType<UserAccessResolvedDomainEvent>();
    }

    // ============================================================
    // ResolveExistingAccess
    // ============================================================

    [Fact]
    public void ResolveExistingAccess_ShouldEmitUserAccessResolvedEvent_WhenExternalIdMatches()
    {
        var (tenant, adminRoleId) = CreateTenantWithRoles();
        var invitation = CreateUsedInvitation(tenant, adminRoleId);
        var user = User.CreateFromInvitation(invitation, ExternalId.Create("oid-legitimate"), "admin");
        user.ClearDomainEvents();

        user.ResolveExistingAccess(ExternalId.Create("oid-legitimate"));

        var evt = user.DomainEvents.OfType<UserAccessResolvedDomainEvent>().Single();
        evt.TenantId.Should().Be(tenant.Id);
        evt.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void ResolveExistingAccess_ShouldThrow_WhenExternalIdDoesNotMatch()
    {
        // Security invariant: same email but different OID means a different identity
        // provider account is trying to claim the same user slot — must be rejected.
        var (tenant, adminRoleId) = CreateTenantWithRoles();
        var invitation = CreateUsedInvitation(tenant, adminRoleId);
        var user = User.CreateFromInvitation(invitation, ExternalId.Create("oid-legitimate"), "admin");

        var act = () => user.ResolveExistingAccess(ExternalId.Create("oid-attacker"));

        act.Should().Throw<UserAlreadyExistsException>();
    }
}
