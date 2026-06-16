using Atlas.Party.Domain.Parties;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Commands.UpdateIndividual;

public sealed record UpdateIndividualCommand(
    Guid PartyId,
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly? BirthDate,
    Gender? Gender,
    IReadOnlyList<AddressInput> Addresses
);
