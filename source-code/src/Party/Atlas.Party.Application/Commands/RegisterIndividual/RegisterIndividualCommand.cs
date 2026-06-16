using Atlas.Party.Domain.Parties;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Commands.RegisterIndividual;

public sealed record RegisterIndividualCommand(
    string TaxNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly? BirthDate,
    Gender? Gender,
    IReadOnlyList<AddressInput> Addresses
);
