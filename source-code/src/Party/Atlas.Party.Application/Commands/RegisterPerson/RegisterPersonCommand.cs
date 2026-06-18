using Atlas.Party.Domain.Parties;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Commands.RegisterPerson;

public sealed record RegisterPersonCommand(
    string TaxNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly? BirthDate,
    Gender? Gender,
    IReadOnlyList<AddressInput> Addresses,
    IReadOnlyList<ContactInput> Contacts
);

