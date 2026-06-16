using Atlas.Party.Domain.Shared.Exceptions;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Shared;

/// <summary>
/// Brazilian postal address value object.
/// </summary>
public sealed class PostalAddress : ValueObject
{
    public string Street { get; }
    public string Number { get; }
    public string? Complement { get; }
    public string District { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    private PostalAddress(
        string street, string number, string? complement,
        string district, string city, string state,
        string zipCode, string country)
    {
        Street = street;
        Number = number;
        Complement = complement;
        District = district;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    public static PostalAddress Create(
        string street, string number, string? complement,
        string district, string city, string state,
        string zipCode, string country = "BR")
    {
        if (string.IsNullOrWhiteSpace(street))   throw new InvalidPostalAddressException(nameof(street));
        if (string.IsNullOrWhiteSpace(number))   throw new InvalidPostalAddressException(nameof(number));
        if (string.IsNullOrWhiteSpace(district)) throw new InvalidPostalAddressException(nameof(district));
        if (string.IsNullOrWhiteSpace(city))     throw new InvalidPostalAddressException(nameof(city));
        if (string.IsNullOrWhiteSpace(state))    throw new InvalidPostalAddressException(nameof(state));

        var cleanZip = new string(zipCode.Where(char.IsDigit).ToArray());
        if (cleanZip.Length != 8)
            throw new InvalidPostalAddressException(nameof(zipCode));

        return new PostalAddress(
            street.Trim(), number.Trim(), complement?.Trim(),
            district.Trim(), city.Trim(), state.Trim().ToUpperInvariant(),
            cleanZip, country.Trim().ToUpperInvariant());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return Number;
        yield return Complement ?? string.Empty;
        yield return District;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }
}
