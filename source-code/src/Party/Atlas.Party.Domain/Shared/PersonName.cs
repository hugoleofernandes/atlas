using Atlas.Party.Domain.Shared.Exceptions;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Shared;

/// <summary>
/// Full name of a natural person.
/// </summary>
public sealed class PersonName : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }
    public string? MiddleName { get; }

    private PersonName(string firstName, string lastName, string? middleName)
    {
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
    }

    public static PersonName Create(string firstName, string lastName, string? middleName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new InvalidPersonNameException(nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new InvalidPersonNameException(nameof(lastName));

        return new PersonName(firstName.Trim(), lastName.Trim(), middleName?.Trim());
    }

    public string FullName => MiddleName is null
        ? $"{FirstName} {LastName}"
        : $"{FirstName} {MiddleName} {LastName}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return MiddleName ?? string.Empty;
    }

    public override string ToString() => FullName;
}
