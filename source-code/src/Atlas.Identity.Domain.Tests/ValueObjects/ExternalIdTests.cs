using Atlas.Identity.Domain.ValueObjects;
using Atlas.Identity.Domain.ValueObjects.Exceptions;
using FluentAssertions;

namespace Atlas.Identity.Tests.ValueObjects;

public sealed class ExternalIdTests
{
    // ------------------------------------------------------------
    // 1. Happy Path
    // ------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnExternalId_WhenValueIsProvided()
    {
        var id = ExternalId.Create("aad-oid-abc123");

        id.Value.Should().Be("aad-oid-abc123");
    }

    [Fact]
    public void Create_ShouldPreserveOriginalCase_WhenValueProvided()
    {
        // ExternalId is an opaque identifier from the IdP — must NOT be normalized
        var id = ExternalId.Create("ABC-DEF-123");

        id.Value.Should().Be("ABC-DEF-123");
    }

    [Fact]
    public void Create_ShouldAcceptGuidFormat_WhenValueIsGuid()
    {
        var guid = Guid.NewGuid().ToString();

        var id = ExternalId.Create(guid);

        id.Value.Should().Be(guid);
    }

    // ------------------------------------------------------------
    // 2. Validation
    // ------------------------------------------------------------

    [Fact]
    public void Create_ShouldThrow_WhenValueIsEmpty()
    {
        var act = () => ExternalId.Create("");

        act.Should().Throw<InvalidExternalIdException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenValueIsWhitespace()
    {
        var act = () => ExternalId.Create("   ");

        act.Should().Throw<InvalidExternalIdException>();
    }

    // ------------------------------------------------------------
    // 3. Equality
    // ------------------------------------------------------------

    [Fact]
    public void Equality_ShouldBeTrue_WhenValuesAreIdentical()
    {
        var a = ExternalId.Create("same-oid");
        var b = ExternalId.Create("same-oid");

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_ShouldBeFalse_WhenValuesDiffer()
    {
        var a = ExternalId.Create("oid-user-a");
        var b = ExternalId.Create("oid-user-b");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_ShouldBeCaseSensitive_WhenValuesHaveDifferentCase()
    {
        // OIDs from identity providers are case-sensitive opaque strings
        var a = ExternalId.Create("ABC");
        var b = ExternalId.Create("abc");

        a.Should().NotBe(b);
    }

    // ------------------------------------------------------------
    // 4. Implicit Conversion
    // ------------------------------------------------------------

    [Fact]
    public void ImplicitConversion_ShouldReturnValue_WhenConvertedToString()
    {
        var id = ExternalId.Create("oid-123");

        string value = id;

        value.Should().Be("oid-123");
    }
}
