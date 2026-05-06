using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.ValueObjects;
using FluentAssertions;

namespace Atlas.Identity.Tests.Tenants;

public sealed class InvitationTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    private static Invitation CreateInvitation(TimeSpan ttl)
    {
        return new Invitation(
            TenantId,
            Email.Create("user@test.com"),
            Role.Create("admin"),
            InvitationTtl.Create(ttl)
        );
    }

    // ------------------------------------------------------------
    // 1. Happy Path
    // ------------------------------------------------------------
    [Fact]
    public void Use_ShouldMarkInvitationAsUsed_WhenInvitationIsActive()
    {
        // Arrange
        var invitation = CreateInvitation(TimeSpan.FromHours(1));

        // Act
        invitation.Use();

        // Assert
        invitation.IsUsed.Should().BeTrue();
        invitation.IsActive.Should().BeFalse();
    }

    // ------------------------------------------------------------
    // 2. Cannot Use Twice
    // ------------------------------------------------------------
    [Fact]
    public void Use_ShouldThrow_WhenInvitationAlreadyUsed()
    {
        // Arrange
        var invitation = CreateInvitation(TimeSpan.FromHours(1));
        invitation.Use();

        // Act
        var act = () => invitation.Use();

        // Assert
        act.Should().Throw<InvitationAlreadyUsedException>();
    }

    // ------------------------------------------------------------
    // 3. Cannot Use After Expiration
    // ------------------------------------------------------------
    [Fact]
    public void Use_ShouldThrow_WhenInvitationIsExpired()
    {
        // Arrange
        var invitation = CreateInvitation(TimeSpan.FromHours(1));

        // Force expiration (allowed in test setup)
        typeof(Invitation)
            .GetProperty(nameof(Invitation.ExpiresAt))!
            .SetValue(invitation, DateTime.UtcNow.AddSeconds(-1));

        // Act
        var act = () => invitation.Use();

        // Assert
        act.Should().Throw<InvitationExpiredException>();
    }

    // ------------------------------------------------------------
    // 4. IsExpired ShouldBeTrue_WhenNowIsAfterExpiresAt
    // ------------------------------------------------------------
    [Fact]
    public void IsExpired_ShouldBeTrue_WhenCurrentTimeIsAfterExpiration()
    {
        // Arrange
        var invitation = CreateInvitation(TimeSpan.FromHours(1));

        typeof(Invitation)
            .GetProperty(nameof(Invitation.ExpiresAt))!
            .SetValue(invitation, DateTime.UtcNow.AddSeconds(-1));

        // Act
        var result = invitation.IsExpired;

        // Assert
        result.Should().BeTrue();
    }

    // ------------------------------------------------------------
    // 5. IsActive ShouldBeFalse_WhenExpired
    // ------------------------------------------------------------
    [Fact]
    public void IsActive_ShouldBeFalse_WhenInvitationIsExpired()
    {
        // Arrange
        var invitation = CreateInvitation(TimeSpan.FromHours(1));

        typeof(Invitation)
            .GetProperty(nameof(Invitation.ExpiresAt))!
            .SetValue(invitation, DateTime.UtcNow.AddSeconds(-1));

        // Act
        var result = invitation.IsActive;

        // Assert
        result.Should().BeFalse();
    }

    // ------------------------------------------------------------
    // 6. IsActive ShouldBeFalse_WhenUsed
    // ------------------------------------------------------------
    [Fact]
    public void IsActive_ShouldBeFalse_WhenInvitationIsUsed()
    {
        // Arrange
        var invitation = CreateInvitation(TimeSpan.FromHours(1));
        invitation.Use();

        // Act
        var result = invitation.IsActive;

        // Assert
        result.Should().BeFalse();
    }

    // ------------------------------------------------------------
    // 7. IsActive ShouldBeTrue_WhenNotUsedAndNotExpired
    // ------------------------------------------------------------
    [Fact]
    public void IsActive_ShouldBeTrue_WhenInvitationIsValid()
    {
        // Arrange
        var invitation = CreateInvitation(TimeSpan.FromHours(1));

        // Act
        var result = invitation.IsActive;

        // Assert
        result.Should().BeTrue();
    }
}
