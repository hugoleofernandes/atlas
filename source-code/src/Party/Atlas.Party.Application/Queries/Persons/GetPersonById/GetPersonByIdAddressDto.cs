using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Queries.Persons.GetPersonById;

public sealed record GetPersonByIdAddressDto(
    Guid AddressId,
    AddressType Type,
    string Street,
    string Number,
    string? Complement,
    string District,
    string City,
    string State,
    string ZipCode,
    string Country,
    bool IsPrimary
);
