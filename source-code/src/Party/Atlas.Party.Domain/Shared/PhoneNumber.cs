using Atlas.Party.Domain.Shared.Exceptions;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Shared;

/// <summary>
/// Phone number in E.164 format (e.g. +5511999998888).
/// Accepts input with or without country code; defaults to +55 (Brazil) when absent.
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string raw)
    {
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        var hasPlus = raw.TrimStart().StartsWith('+');

        // If no country code prefix, assume Brazil (+55)
        var e164 = hasPlus
            ? $"+{digits}"
            : digits.Length is 10 or 11
                ? $"+55{digits}"
                : $"+{digits}";

        // E.164: 7–15 digits after +
        var bodyDigits = e164[1..];
        if (bodyDigits.Length < 7 || bodyDigits.Length > 15 || !bodyDigits.All(char.IsDigit))
            throw new InvalidContactInfoException("phone", raw);

        return new PhoneNumber(e164);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
