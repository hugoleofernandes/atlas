using Atlas.Party.Domain.Parties;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Commands.UpdatePerson;

public sealed record UpdatePersonCommand(
    Guid PartyId,
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly? BirthDate,
    Gender? Gender,
    IReadOnlyList<AddressInput> Addresses,
    IReadOnlyList<ContactInput> Contacts
);

