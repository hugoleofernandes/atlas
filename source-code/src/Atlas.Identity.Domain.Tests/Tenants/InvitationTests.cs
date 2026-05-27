using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.Entities.Tenants.Invitations;
using Atlas.Identity.Domain.Entities.Tenants.Invitations.Exceptions;
using FluentAssertions;

namespace Atlas.Identity.Tests.Tenants;

public sealed class InvitationTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid RoleId = Guid.NewGuid();

    private static Invitation CreateInvitation(TimeSpan ttl)
    {
        return new Invitation(
            TenantId,
            Email.Create("user@test.com"),
            RoleId,
            InvitationTtl.Create(ttl)
        );
    }

    // ------------------------------------------------------------
    // 1. Happy Path
    // ------------------------------------------------------------
    [Fact]
    public void Use_ShouldMarkInvitationAsUsed_WhenInvitationIsActive()
    {
        var invitation = CreateInvitation(TimeSpan.FromHours(1));

        invitation.Use();

        invitation.IsUsed.Should().BeTrue();
        invitation.IsActive.Should().BeFalse();
    }

    // ------------------------------------------------------------
    // 2. Cannot Use Twice
    // ------------------------------------------------------------
    [Fact]
    public void Use_ShouldThrow_WhenInvitationAlreadyUsed()
    {
        var invitation = CreateInvitation(TimeSpan.FromHours(1));
        invitation.Use();

        var act = () => invitation.Use();

        act.Should().Throw<InvitationAlreadyUsedException>();
    }

    // ------------------------------------------------------------
    // 3. Cannot Use After Expiration
    // ------------------------------------------------------------
    [Fact]
    public void Use_ShouldThrow_WhenInvitationIsExpired()
    {
        var invitation = CreateInvitation(TimeSpan.FromHours(1));

        typeof(Invitation)
            .GetProperty(nameof(Invitation.ExpiresAt))!
            .SetValue(invitation, DateTime.UtcNow.AddSeconds(-1));

        var act = () => invitation.Use();

        act.Should().Throw<InvitationExpiredException>();
    }

    // ------------------------------------------------------------
    // 4. IsExpired
    // ------------------------------------------------------------
    [Fact]
    public void IsExpired_ShouldBeTrue_WhenCurrentTimeIsAfterExpiration()
    {
        var invitation = CreateInvitation(TimeSpan.FromHours(1));

        typeof(Invitation)
            .GetProperty(nameof(Invitation.ExpiresAt))!
            .SetValue(invitation, DateTime.UtcNow.AddSeconds(-1));

        invitation.IsExpired.Should().BeTrue();
    }

    // ------------------------------------------------------------
    // 5. IsActive
    // ------------------------------------------------------------
    [Fact]
    public void IsActive_ShouldBeFalse_WhenInvitationIsExpired()
    {
        var invitation = CreateInvitation(TimeSpan.FromHours(1));

        typeof(Invitation)
            .GetProperty(nameof(Invitation.ExpiresAt))!
            .SetValue(invitation, DateTime.UtcNow.AddSeconds(-1));

        invitation.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldBeFalse_WhenInvitationIsUsed()
    {
        var invitation = CreateInvitation(TimeSpan.FromHours(1));
        invitation.Use();

        invitation.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldBeTrue_WhenInvitationIsValid()
    {
        var invitation = CreateInvitation(TimeSpan.FromHours(1));

        invitation.IsActive.Should().BeTrue();
    }

    // ------------------------------------------------------------
    // 6. TenantRoleId is persisted
    // ------------------------------------------------------------
    [Fact]
    public void Invitation_ShouldStoreRoleId_WhenCreated()
    {
        var invitation = CreateInvitation(TimeSpan.FromHours(1));

        invitation.RoleId.Should().Be(RoleId);
    }
}
