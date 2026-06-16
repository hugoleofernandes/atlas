using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Queries.Individuals;
using Atlas.Party.Application.Queries.Individuals.GetIndividualById;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Individuals.GetIndividualById;

/// <summary>
/// Returns a single individual by id. Returns both active and inactive individuals.
/// </summary>
public sealed class GetIndividualByIdEndpoint(IGetIndividualByIdQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<GetIndividualByIdRequest, IndividualDto>
{
    public override void Configure()
    {
        Get("bff/v1/party/individuals/{id}");
        Policies($"permission:{PartyModulePermissions.Individual.Read.Code}");
        Description(d => d.Produces<IndividualDto>());
    }

    public override async Task HandleAsync(GetIndividualByIdRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new GetIndividualByIdQuery(req.Id), ct);
        await OkFromResultAsync(result, ct);
    }
}
