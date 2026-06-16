using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Roles;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Tenants._Roles;
using Atlas.Identity.Domain.Users;
using Atlas.Identity.Domain.Users.Events;
using Atlas.Identity.Domain.Users.Exceptions;
using Atlas.Platform.Domain.Tenants;
using FluentAssertions;

namespace Atlas.Identity.Tests.Tenants;

public class UserTests
{
    private static (Tenant tenant, Guid adminRoleId) CreateTenantWithRoles()
    {
        var tenant = new Tenant("test");
        var adminRole = Role.Create(
            tenant.Id,
            "admin",
            PermissionFixtures.Many(3),
            isSystem: true,
            id: SystemRoleIds.Admin
        );
        return (tenant, adminRole.Id);
    }

    private static Invitation CreateUsedInvitation(Tenant tenant, Guid roleId, string email = "user@test.com")
    {
        var invitation = Invitation.Create(
            tenant.Id,
            Email.Create(email),
            roleId,
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );
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
        // provider account is trying to claim the same user slot - must be rejected.
        var (tenant, adminRoleId) = CreateTenantWithRoles();
        var invitation = CreateUsedInvitation(tenant, adminRoleId);
        var user = User.CreateFromInvitation(invitation, ExternalId.Create("oid-legitimate"), "admin");

        var act = () => user.ResolveExistingAccess(ExternalId.Create("oid-attacker"));

        act.Should().Throw<UserIdentityMismatchException>();
    }
}
