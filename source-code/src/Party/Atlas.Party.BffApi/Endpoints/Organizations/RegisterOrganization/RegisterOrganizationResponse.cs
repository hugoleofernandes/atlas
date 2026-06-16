using Atlas.Party.Application.Commands.RegisterOrganization;

namespace Atlas.Party.BffApi.Endpoints.Organizations.RegisterOrganization;

public sealed record RegisterOrganizationResponse(Guid PartyId, string TaxNumber, string LegalName)
{
    public static RegisterOrganizationResponse From(RegisterOrganizationOutput output)
        => new(output.PartyId, output.TaxNumber, output.LegalName);
}
