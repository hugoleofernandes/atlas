using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Queries.Organizations;
using Atlas.Party.Application.Queries.Organizations.ListOrganizations;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Organizations.ListOrganizations;

public sealed class ListOrganizationsEndpoint(IListOrganizationsQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<ListOrganizationsRequest, IReadOnlyList<OrganizationDto>>
{
    public override void Configure()
    {
        Get("bff/v1/party/organizations");
        Policies($"permission:{PartyModulePermissions.Organization.Read.Code}");
        Description(d => d.Produces<IReadOnlyList<OrganizationDto>>());
    }

    public override async Task HandleAsync(ListOrganizationsRequest req, CancellationToken ct)
    {
        var query = new ListOrganizationsQuery(req.IsActive);
        var result = await invoker.InvokeAsync(handler, query, ct);
        await OkFromResultAsync(result, ct);
    }
}
