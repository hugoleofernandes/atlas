using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Invitations.Events;
using Atlas.Identity.Domain.Invitations.Exceptions;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Tenants;
using Atlas.Identity.Domain.Tenants.Roles.Exceptions;
using Atlas.Identity.Domain.Tenants.Roles.Permissions;
using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.Staff.Domain.Permissions;
using FluentAssertions;

namespace Atlas.Identity.Tests.Tenants;

public class InvitationTests
{
    private static readonly IReadOnlySet<string> AllCodes = new HashSet<string>
    {
        IdentityPermissions.Tenant.Roles.Read,
        IdentityPermissions.Tenant.Roles.Create,
        IdentityPermissions.Tenant.Roles.Update,
        IdentityPermissions.Tenant.Roles.Delete,
        IdentityPermissions.Tenant.Roles.Manage,
        IdentityPermissions.Tenant.Invitations.Read,
        IdentityPermissions.Tenant.Invitations.Create,
        IdentityPermissions.Tenant.Invitations.Update,
        IdentityPermissions.Tenant.Invitations.Delete,
        IdentityPermissions.Tenant.Invitations.Manage,
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

    private static void ForceExpire(Invitation invitation)
    {
        typeof(Invitation)
            .GetProperty(nameof(Invitation.ExpiresAt))!
            .SetValue(invitation, DateTime.UtcNow.AddSeconds(-1));
    }

    private static (Tenant tenant, Guid adminRoleId) CreateTenantWithRoles()
    {
        var tenant = new Tenant("test");
        tenant.SeedDefaultRoles(AllCodes, AllIncludingSystemCodes, DefaultMemberPermissions);
        tenant.ClearDomainEvents();
        return (tenant, tenant.Roles.Single(r => r.Name == "admin").Id);
    }

    private static Invitation MakeInvitation(Guid tenantId, Guid roleId, string email = "user@test.com")
        => Invitation.Create(tenantId, Email.Create(email), roleId, InvitationTtl.Create(TimeSpan.FromHours(1)));

    // ============================================================
    // 1. CREATE (FACTORY)
    // ============================================================

    [Fact]
    public void Create_ShouldReturnInvitation_WithCorrectData()
    {
        var (tenant, adminRoleId) = CreateTenantWithRoles();

        var invitation = MakeInvitation(tenant.Id, adminRoleId);

        invitation.TenantId.Should().Be(tenant.Id);
        invitation.RoleId.Should().Be(adminRoleId);
        invitation.Email.Value.Should().Be("user@test.com");
        invitation.IsUsed.Should().BeFalse();
        invitation.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldEmitUserInvitedEvent()
    {
        var (tenant, adminRoleId) = CreateTenantWithRoles();

        var invitation = MakeInvitation(tenant.Id, adminRoleId);

        var evt = invitation.DomainEvents.OfType<UserInvitedDomainEvent>().Single();
        evt.TenantId.Should().Be(tenant.Id);
        evt.Email.Should().Be("user@test.com");
    }

    // ============================================================
    // 2. USE()
    // ============================================================

    [Fact]
    public void Use_ShouldMarkIsUsed_WhenInvitationIsActive()
    {
        var (tenant, adminRoleId) = CreateTenantWithRoles();
        var invitation = MakeInvitation(tenant.Id, adminRoleId);

        invitation.Use();

        invitation.IsUsed.Should().BeTrue();
    }

    [Fact]
    public void Use_ShouldEmitInvitationUsedEvent_WhenSuccessful()
    {
        var (tenant, adminRoleId) = CreateTenantWithRoles();
        var invitation = MakeInvitation(tenant.Id, adminRoleId);

        invitation.Use();

        var evt = invitation.DomainEvents.OfType<InvitationUsedDomainEvent>().Single();
        evt.TenantId.Should().Be(tenant.Id);
        evt.UserId.Should().Be(invitation.Id);
        evt.Email.Should().Be("user@test.com");
    }

    [Fact]
    public void Use_ShouldThrow_WhenInvitationIsAlreadyUsed()
    {
        var (tenant, adminRoleId) = CreateTenantWithRoles();
        var invitation = MakeInvitation(tenant.Id, adminRoleId);
        invitation.Use();

        var act = () => invitation.Use();

        act.Should().Throw<InvitationAlreadyUsedException>();
    }

    [Fact]
    public void Use_ShouldThrow_WhenInvitationIsExpired()
    {
        var (tenant, adminRoleId) = CreateTenantWithRoles();
        var invitation = MakeInvitation(tenant.Id, adminRoleId);
        ForceExpire(invitation);

        var act = () => invitation.Use();

        act.Should().Throw<InvitationExpiredException>();
    }

    // ============================================================
    // 3. ISACTIVE
    // ============================================================

    [Fact]
    public void IsActive_ShouldBeFalse_WhenInvitationIsUsed()
    {
        var (tenant, adminRoleId) = CreateTenantWithRoles();
        var invitation = MakeInvitation(tenant.Id, adminRoleId);
        invitation.Use();

        invitation.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldBeFalse_WhenInvitationIsExpired()
    {
        var (tenant, adminRoleId) = CreateTenantWithRoles();
        var invitation = MakeInvitation(tenant.Id, adminRoleId);
        ForceExpire(invitation);

        invitation.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldBeTrue_WhenInvitationIsNeitherUsedNorExpired()
    {
        var (tenant, adminRoleId) = CreateTenantWithRoles();
        var invitation = MakeInvitation(tenant.Id, adminRoleId);

        invitation.IsActive.Should().BeTrue();
    }
}
