using Atlas.Identity.Domain.ValueObjects;
using Atlas.Identity.Domain.ValueObjects.Exceptions;
using FluentAssertions;

namespace Atlas.Identity.Tests.ValueObjects;

public sealed class RoleTests
{
    // ------------------------------------------------------------
    // 1. Happy Path — all allowed roles
    // ------------------------------------------------------------

    [Theory]
    [InlineData("admin")]
    [InlineData("member")]
    [InlineData("owner")]
    public void Create_ShouldReturnRole_WhenRoleIsAllowed(string allowedRole)
    {
        var role = Role.Create(allowedRole);

        role.Value.Should().Be(allowedRole);
    }

    [Fact]
    public void Create_ShouldNormalizeToLowercase_WhenRoleHasUppercase()
    {
        var role = Role.Create("ADMIN");

        role.Value.Should().Be("admin");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace_WhenRoleHasPadding()
    {
        var role = Role.Create("  member  ");

        role.Value.Should().Be("member");
    }

    // ------------------------------------------------------------
    // 2. Validation
    // ------------------------------------------------------------

    [Fact]
    public void Create_ShouldThrow_WhenRoleIsEmpty()
    {
        var act = () => Role.Create("");

        act.Should().Throw<InvalidRoleException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenRoleIsWhitespace()
    {
        var act = () => Role.Create("   ");

        act.Should().Throw<InvalidRoleException>();
    }

    [Theory]
    [InlineData("superuser")]
    [InlineData("guest")]
    [InlineData("root")]
    [InlineData("viewer")]
    public void Create_ShouldThrow_WhenRoleIsNotAllowed(string invalidRole)
    {
        var act = () => Role.Create(invalidRole);

        act.Should().Throw<InvalidRoleException>();
    }

    // ------------------------------------------------------------
    // 3. Equality
    // ------------------------------------------------------------

    [Fact]
    public void Equality_ShouldBeTrue_WhenRolesAreIdentical()
    {
        var a = Role.Create("admin");
        var b = Role.Create("admin");

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_ShouldBeTrue_WhenRolesDifferOnlyInCase()
    {
        var a = Role.Create("ADMIN");
        var b = Role.Create("admin");

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_ShouldBeFalse_WhenRolesDiffer()
    {
        var a = Role.Create("admin");
        var b = Role.Create("member");

        a.Should().NotBe(b);
    }

    // ------------------------------------------------------------
    // 4. Implicit Conversion
    // ------------------------------------------------------------

    [Fact]
    public void ImplicitConversion_ShouldReturnValue_WhenConvertedToString()
    {
        var role = Role.Create("admin");

        string value = role;

        value.Should().Be("admin");
    }
}
