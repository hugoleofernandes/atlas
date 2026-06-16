using Atlas.Party.BffApi.Endpoints.Shared;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.BffApi.Endpoints.Organizations.RegisterOrganization;

public sealed record RegisterOrganizationRequest(
    string TaxNumber,
    string LegalName,
    string? TradeName,
    LegalType LegalType,
    IReadOnlyList<AddressRequest>? Addresses
);
