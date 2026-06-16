using Atlas.Party.Domain.Parties;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Commands.RegisterOrganization;

public sealed record RegisterOrganizationCommand(
    string TaxNumber,
    string LegalName,
    string? TradeName,
    LegalType LegalType,
    IReadOnlyList<AddressInput> Addresses
);
