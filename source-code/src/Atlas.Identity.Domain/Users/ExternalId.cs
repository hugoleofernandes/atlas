using Atlas.Identity.Domain.Users.Exceptions;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Users;

/// <summary>
/// Represents the external identity of a user in an identity provider (OIDC / Azure AD).
///
/// Invariants:
/// - Must not be null or empty.
/// - Represents the unique identifier of the user in the external identity provider.
///
/// Purpose:
/// - Encapsulates the identity provider user identifier.
/// - Ensures consistent handling of external identity across the domain.
/// </summary>
public sealed class ExternalId : ValueObject
{
    public string Value { get; }

    private ExternalId(string value)
    {
        Value = value;
    }

    public static ExternalId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidExternalIdException(value);

        return new ExternalId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(ExternalId id) => id.Value;
}
