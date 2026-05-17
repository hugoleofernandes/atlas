using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.Entities.Tenants.Events;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.ValueObjects;
using FluentAssertions;

namespace Atlas.Identity.Tests.Tenants;

public class TenantTests
{
    // ============================================================
    // HELPER
    // ============================================================

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
    // 2. LIFECYCLE: DEACTIVATE
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
    // 3. INVITE USER
    // ============================================================

    [Fact]
    public void InviteUser_ShouldThrow_WhenTenantIsInactive()
    {
        var tenant = new Tenant("test");
        tenant.Deactivate();

        var act = () => tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        act.Should().Throw<TenantInactiveException>();
    }

    [Fact]
    public void InviteUser_ShouldThrow_WhenUserAlreadyExists()
    {
        var tenant = new Tenant("test");

        tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("user@test.com"));

        var act = () => tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        act.Should().Throw<UserAlreadyExistsException>();
    }

    [Fact]
    public void InviteUser_ShouldThrow_WhenActiveInvitationAlreadyExists()
    {
        var tenant = new Tenant("test");

        tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        var act = () => tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        act.Should().Throw<DuplicateInvitationException>();
    }

    [Fact]
    public void InviteUser_ShouldSucceed_WhenPreviousInvitationIsExpired()
    {
        // Invitation expires before the user ever accesses the system.
        // The tenant should be allowed to send a fresh invitation.
        var tenant = new Tenant("test");

        tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        ForceExpire(tenant.Invitations.Single());

        var act = () => tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("member"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        act.Should().NotThrow();
        tenant.Invitations.Should().HaveCount(2);
    }

    [Fact]
    public void InviteUser_ShouldEmitUserInvitedEvent_WhenValid()
    {
        var tenant = new Tenant("test");

        tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        tenant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserInvitedDomainEvent>();
    }

    [Fact]
    public void InviteUser_ShouldEmitEventWithCorrectData_WhenValid()
    {
        var tenant = new Tenant("test");

        tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        var evt = tenant.DomainEvents.OfType<UserInvitedDomainEvent>().Single();
        evt.TenantId.Should().Be(tenant.Id);
        evt.Email.Should().Be("user@test.com");
    }

    [Fact]
    public void InviteUser_ShouldAddInvitation_WhenValid()
    {
        var tenant = new Tenant("test");

        tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        tenant.Invitations.Should().ContainSingle(i => i.Email.Value == "user@test.com");
    }

    // ============================================================
    // 4. RESOLVE ACCESS
    // ============================================================

    [Fact]
    public void ResolveAccess_ShouldReturnExistingUser_WhenUserAlreadyExists()
    {
        var tenant = new Tenant("test");

        tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromDays(1))
        );

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
        var tenant = new Tenant("test");
        tenant.Deactivate();

        var act = () => tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("user@test.com"));

        act.Should().Throw<TenantInactiveException>();
    }

    [Fact]
    public void ResolveAccess_ShouldThrow_WhenInvitationDoesNotExist()
    {
        var tenant = new Tenant("test");

        var act = () => tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("missing@test.com"));

        act.Should().Throw<InvitationNotFoundException>();
    }

    [Fact]
    public void ResolveAccess_ShouldThrow_WhenInvitationIsExpired()
    {
        var tenant = new Tenant("test");

        tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        ForceExpire(tenant.Invitations.Single());

        var act = () => tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("user@test.com"));

        act.Should().Throw<InvitationExpiredException>();
    }

    [Fact]
    public void ResolveAccess_ShouldThrow_WhenUserAlreadyExistsAfterInvitationUse()
    {
        var tenant = new Tenant("test");

        tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

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
        var tenant = new Tenant("test");

        tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        tenant.ResolveAccess(ExternalId.Create("oid-legitimate"), Email.Create("user@test.com"));
        tenant.ClearDomainEvents();

        var act = () => tenant.ResolveAccess(ExternalId.Create("oid-attacker"), Email.Create("user@test.com"));

        act.Should().Throw<UserAlreadyExistsException>();
    }

    [Fact]
    public void ResolveAccess_ShouldCreateUser_WhenInvitationIsValid()
    {
        var tenant = new Tenant("test");

        tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

        var user = tenant.ResolveAccess(ExternalId.Create("oid-1"), Email.Create("user@test.com"));

        user.Email.Value.Should().Be("user@test.com");
        tenant.Users.Should().Contain(user);
    }

    [Fact]
    public void ResolveAccess_ShouldEmitEvents_WhenUserIsCreated()
    {
        var tenant = new Tenant("test");

        tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

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
        var tenant = new Tenant("test");

        tenant.InviteUser(
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(1))
        );

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
