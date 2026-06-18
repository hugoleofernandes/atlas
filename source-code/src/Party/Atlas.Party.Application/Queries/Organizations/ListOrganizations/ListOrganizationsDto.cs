using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Queries.Organizations.ListOrganizations;

public sealed record ListOrganizationsDto(
    Guid PartyId,
    string TaxNumber,
    string LegalName,
    string? TradeName,
    LegalType LegalType,
    bool IsActive,
    IReadOnlyList<ListOrganizationsAddressDto> Addresses,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByEmail,
    DateTime? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByEmail
);
