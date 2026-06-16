using Atlas.Party.Application.Commands.UpdateOrganization;

namespace Atlas.Party.BffApi.Endpoints.Organizations.UpdateOrganization;

public sealed record UpdateOrganizationResponse(Guid PartyId, string LegalName)
{
    public static UpdateOrganizationResponse From(UpdateOrganizationOutput output)
        => new(output.PartyId, output.LegalName);
}
