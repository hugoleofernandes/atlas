using Atlas.Party.Domain.Shared.Exceptions;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Shared;

/// <summary>
/// Email address value object for contact information on a Party.
/// Validates RFC-5322 format via a simple structural check.
/// </summary>
public sealed class EmailAddress : ValueObject
{
    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static EmailAddress Create(string raw)
    {
        var trimmed = raw?.Trim() ?? string.Empty;
        var atIndex = trimmed.IndexOf('@');

        if (atIndex <= 0 || atIndex == trimmed.Length - 1 || trimmed.LastIndexOf('.') <= atIndex)
            throw new InvalidContactInfoException("email", raw ?? string.Empty);

        return new EmailAddress(trimmed.ToLowerInvariant());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
