namespace Atlas.Party.Application.Commands.RegisterOrganization;

public sealed record RegisterOrganizationOutput(Guid PartyId, string TaxNumber, string LegalName);
