using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Invitations.Exceptions;
using FluentAssertions;

namespace Atlas.Identity.Tests.ValueObjects;

public sealed class InvitationTtlTests
{
    // ------------------------------------------------------------
    // 1. Happy Path
    // ------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnTtl_WhenValueIsPositive()
    {
        var ttl = InvitationTtl.Create(TimeSpan.FromHours(24));

        ttl.Value.Should().Be(TimeSpan.FromHours(24));
    }

    [Fact]
    public void Create_ShouldSucceed_WhenTtlIsExactlyMaximum()
    {
        // Boundary: 30 days is the maximum allowed TTL
        var act = () => InvitationTtl.Create(TimeSpan.FromDays(30));

        act.Should().NotThrow();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenTtlIsMinimalPositive()
    {
        // Boundary: any positive duration should be accepted
        var act = () => InvitationTtl.Create(TimeSpan.FromSeconds(1));

        act.Should().NotThrow();
    }

    // ------------------------------------------------------------
    // 2. Validation
    // ------------------------------------------------------------

    [Fact]
    public void Create_ShouldThrow_WhenTtlIsZero()
    {
        var act = () => InvitationTtl.Create(TimeSpan.Zero);

        act.Should().Throw<InvalidInvitationTtlException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenTtlIsNegative()
    {
        var act = () => InvitationTtl.Create(TimeSpan.FromMinutes(-1));

        act.Should().Throw<InvalidInvitationTtlException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenTtlExceedsMaximum()
    {
        // Boundary: 30 days + 1 second exceeds the limit
        var act = () => InvitationTtl.Create(TimeSpan.FromDays(30).Add(TimeSpan.FromSeconds(1)));

        act.Should().Throw<InvalidInvitationTtlException>();
    }

    // ------------------------------------------------------------
    // 3. Equality
    // ------------------------------------------------------------

    [Fact]
    public void Equality_ShouldBeTrue_WhenDurationsAreIdentical()
    {
        var a = InvitationTtl.Create(TimeSpan.FromHours(48));
        var b = InvitationTtl.Create(TimeSpan.FromHours(48));

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_ShouldBeFalse_WhenDurationsDiffer()
    {
        var a = InvitationTtl.Create(TimeSpan.FromHours(24));
        var b = InvitationTtl.Create(TimeSpan.FromHours(48));

        a.Should().NotBe(b);
    }
}
