using Atlas.Party.Domain.Parties;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Application.Commands.UpdateOrganization;

public sealed record UpdateOrganizationCommand(
    Guid PartyId,
    string LegalName,
    string? TradeName,
    LegalType LegalType,
    IReadOnlyList<AddressInput> Addresses
);
