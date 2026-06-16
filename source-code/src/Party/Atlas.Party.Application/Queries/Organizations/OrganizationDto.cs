using Atlas.Party.Application.Queries.Shared;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Queries.Organizations;

public sealed record OrganizationDto(
    Guid PartyId,
    string TaxNumber,
    string LegalName,
    string? TradeName,
    LegalType LegalType,
    bool IsActive,
    IReadOnlyList<AddressDto> Addresses,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByEmail,
    DateTime? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByEmail
);
