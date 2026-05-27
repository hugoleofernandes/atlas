using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using FluentAssertions;

namespace Atlas.Identity.Tests.ValueObjects;

public sealed class EmailTests
{
    // ------------------------------------------------------------
    // 1. Happy Path
    // ------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnEmail_WhenValidEmailProvided()
    {
        var email = Email.Create("user@example.com");

        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Create_ShouldNormalizeToLowercase_WhenEmailHasUppercase()
    {
        var email = Email.Create("User@Example.COM");

        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace_WhenEmailHasPadding()
    {
        var email = Email.Create("  user@example.com  ");

        email.Value.Should().Be("user@example.com");
    }

    // ------------------------------------------------------------
    // 2. Format Validation
    // ------------------------------------------------------------

    [Fact]
    public void Create_ShouldThrow_WhenEmailIsEmpty()
    {
        var act = () => Email.Create("");

        act.Should().Throw<InvalidEmailException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenEmailIsWhitespace()
    {
        var act = () => Email.Create("   ");

        act.Should().Throw<InvalidEmailException>();
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain.com")]
    [InlineData("nodomain@")]
    [InlineData("missing-at-sign")]
    [InlineData("double@@domain.com")]
    public void Create_ShouldThrow_WhenEmailHasInvalidFormat(string invalid)
    {
        var act = () => Email.Create(invalid);

        act.Should().Throw<InvalidEmailException>();
    }

    // ------------------------------------------------------------
    // 3. Equality
    // ------------------------------------------------------------

    [Fact]
    public void Equality_ShouldBeTrue_WhenEmailsAreIdentical()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("user@example.com");

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_ShouldBeTrue_WhenEmailsDifferOnlyInCase()
    {
        var a = Email.Create("User@Example.COM");
        var b = Email.Create("user@example.com");

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_ShouldBeFalse_WhenEmailsDiffer()
    {
        var a = Email.Create("alice@example.com");
        var b = Email.Create("bob@example.com");

        a.Should().NotBe(b);
    }

    // ------------------------------------------------------------
    // 4. Implicit Conversion
    // ------------------------------------------------------------

    [Fact]
    public void ImplicitConversion_ShouldReturnValue_WhenConvertedToString()
    {
        var email = Email.Create("user@example.com");

        string value = email;

        value.Should().Be("user@example.com");
    }
}
