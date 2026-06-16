namespace Atlas.Party.Application.Commands.DeactivateOrganization;

public sealed record DeactivateOrganizationOutput(Guid PartyId, bool IsActive);
