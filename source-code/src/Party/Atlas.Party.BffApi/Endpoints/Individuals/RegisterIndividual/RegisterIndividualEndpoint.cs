using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Commands.RegisterIndividual;
using Atlas.Party.BffApi.Endpoints.Shared;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Individuals.RegisterIndividual;

/// <summary>
/// Registers a new individual (natural person) for the authenticated user's tenant.
/// </summary>
public sealed class RegisterIndividualEndpoint(IRegisterIndividualCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<RegisterIndividualRequest, RegisterIndividualResponse>
{
    public override void Configure()
    {
        Post("bff/v1/party/individuals");
        Policies($"permission:{PartyModulePermissions.Individual.Create.Code}");
        Description(d => d.Produces<RegisterIndividualResponse>(201));
    }

    public override async Task HandleAsync(RegisterIndividualRequest req, CancellationToken ct)
    {
        var cmd = new RegisterIndividualCommand(
            req.TaxNumber,
            req.FirstName,
            req.LastName,
            req.MiddleName,
            req.BirthDate,
            req.Gender,
            AddressRequestMapper.ToAddressInputs(req.Addresses)
        );
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await CreatedFromResultAsync(result, RegisterIndividualResponse.From, ct);
    }
}
