using Atlas.Party.BffApi.Endpoints.Shared;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.BffApi.Endpoints.Organizations.UpdateOrganization;

public sealed record UpdateOrganizationRequest(
    Guid Id,
    string LegalName,
    string? TradeName,
    LegalType LegalType,
    IReadOnlyList<AddressRequest>? Addresses
);
