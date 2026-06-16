using Atlas.Party.BffApi.Endpoints.Shared;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.BffApi.Endpoints.Individuals.RegisterIndividual;

public sealed record RegisterIndividualRequest(
    string TaxNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly? BirthDate,
    Gender? Gender,
    IReadOnlyList<AddressRequest>? Addresses
);
