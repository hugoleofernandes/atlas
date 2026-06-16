using Atlas.Party.Domain.Shared;

namespace Atlas.Party.BffApi.Endpoints.Shared;

public sealed record AddressRequest(
    AddressType Type,
    string Street,
    string Number,
    string? Complement,
    string District,
    string City,
    string State,
    string ZipCode,
    string? Country,
    bool IsPrimary
);
