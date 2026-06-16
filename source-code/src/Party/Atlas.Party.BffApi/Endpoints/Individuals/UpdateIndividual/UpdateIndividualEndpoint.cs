using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Commands.UpdateIndividual;
using Atlas.Party.BffApi.Endpoints.Shared;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Individuals.UpdateIndividual;

/// <summary>
/// Updates mutable personal details of an existing individual. Does not change TaxNumber or TenantId.
/// </summary>
public sealed class UpdateIndividualEndpoint(IUpdateIndividualCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<UpdateIndividualRequest, UpdateIndividualResponse>
{
    public override void Configure()
    {
        Put("bff/v1/party/individuals/{id}");
        Policies($"permission:{PartyModulePermissions.Individual.Update.Code}");
        Description(d => d.Produces<UpdateIndividualResponse>());
    }

    public override async Task HandleAsync(UpdateIndividualRequest req, CancellationToken ct)
    {
        var cmd = new UpdateIndividualCommand(
            req.Id,
            req.FirstName,
            req.LastName,
            req.MiddleName,
            req.BirthDate,
            req.Gender,
            AddressRequestMapper.ToAddressInputs(req.Addresses)
        );
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await UpdatedFromResultAsync(result, UpdateIndividualResponse.From, ct);
    }
}
