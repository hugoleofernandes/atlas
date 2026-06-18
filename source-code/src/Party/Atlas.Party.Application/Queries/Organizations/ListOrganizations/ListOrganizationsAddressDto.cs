using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Queries.Organizations.ListOrganizations;

public sealed record ListOrganizationsAddressDto(
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
