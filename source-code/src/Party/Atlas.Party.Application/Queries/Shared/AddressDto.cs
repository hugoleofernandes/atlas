using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Queries.Shared;

public sealed record AddressDto(
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
