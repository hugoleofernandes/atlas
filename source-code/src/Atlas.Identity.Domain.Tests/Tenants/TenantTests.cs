using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.Entities.Tenants.Events;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.ValueObjects;
using FluentAssertions;

namespace Atlas.Identity.Tests.Tenants;

public class TenantTests
{
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
            InvitationTtl.Create(TimeSpan.FromDays(1)) // TTL maior
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
            InvitationTtl.Create(TimeSpan.FromMilliseconds(1))
        );

        Thread.Sleep(10);

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
}
