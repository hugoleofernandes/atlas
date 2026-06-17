using Atlas.Party.BffApi.Endpoints.Shared;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.BffApi.Endpoints.Persons.RegisterPerson;

public sealed record RegisterPersonRequest(
    string TaxNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly? BirthDate,
    Gender? Gender,
    IReadOnlyList<AddressRequest>? Addresses
);

