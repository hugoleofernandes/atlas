using Atlas.Party.BffApi.Endpoints.Shared;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.BffApi.Endpoints.Individuals.UpdateIndividual;

public sealed record UpdateIndividualRequest(
    Guid Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly? BirthDate,
    Gender? Gender,
    IReadOnlyList<AddressRequest>? Addresses
);
