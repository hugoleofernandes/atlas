using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.SharedKernel.Domain;
using System.Text.RegularExpressions;

namespace Atlas.Identity.Domain.Shared;

/// <summary>
/// Represents a normalized and validated email address.
///
/// Invariants:
/// - Email must not be null or empty.
/// - Email must be normalized to lowercase.
/// - Email must follow a valid email format.
///
/// Purpose:
/// - Ensures consistent handling of email addresses across the domain.
/// - Prevents primitive obsession and duplicated validation logic.
/// </summary>
public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidEmailException(email);

        var normalized = email.Trim().ToLowerInvariant();

        if (!IsValid(normalized))
            throw new InvalidEmailException(email);

        return new Email(normalized);
    }

    private static bool IsValid(string email)
    {
        return Regex.IsMatch(
            email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
