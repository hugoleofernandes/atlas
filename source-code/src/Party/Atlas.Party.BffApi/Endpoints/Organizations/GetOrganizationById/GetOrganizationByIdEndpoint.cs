using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Queries.Organizations.GetOrganizationById;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Organizations.GetOrganizationById;

/// <summary>
/// Returns a single organization by id. Returns both active and inactive organizations.
/// </summary>
public sealed class GetOrganizationByIdEndpoint(IGetOrganizationByIdQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<GetOrganizationByIdRequest, GetOrganizationByIdDto?>
{
    public override void Configure()
    {
        Get("bff/v1/party/organizations/{id}");
        Policies($"permission:{PartyModulePermissions.Organization.Read.Code}");
        Description(d => d.Produces<GetOrganizationByIdDto>());
    }

    public override async Task HandleAsync(GetOrganizationByIdRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new GetOrganizationByIdQuery(req.Id), ct);
        await OkFromResultAsync(result, ct);
    }
}
