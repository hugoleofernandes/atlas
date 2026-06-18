using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Queries.Organizations.GetOrganizationById;

public sealed record GetOrganizationByIdDto(
    Guid PartyId,
    string TaxNumber,
    string LegalName,
    string? TradeName,
    LegalType LegalType,
    bool IsActive,
    IReadOnlyList<GetOrganizationByIdAddressDto> Addresses,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByEmail,
    DateTime? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByEmail
);
