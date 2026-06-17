using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Commands.RegisterPerson;
using Atlas.Party.BffApi.Endpoints.Shared;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Persons.RegisterPerson;

/// <summary>
/// Registers a new person (natural person) for the authenticated user's tenant.
/// </summary>
public sealed class RegisterPersonEndpoint(IRegisterPersonCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<RegisterPersonRequest, RegisterPersonResponse>
{
    public override void Configure()
    {
        Post("bff/v1/party/persons");
        Policies($"permission:{PartyModulePermissions.Person.Create.Code}");
        Description(d => d.Produces<RegisterPersonResponse>(201));
    }

    public override async Task HandleAsync(RegisterPersonRequest req, CancellationToken ct)
    {
        var cmd = new RegisterPersonCommand(
            req.TaxNumber,
            req.FirstName,
            req.LastName,
            req.MiddleName,
            req.BirthDate,
            req.Gender,
            AddressRequestMapper.ToAddressInputs(req.Addresses)
        );
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await CreatedFromResultAsync(result, RegisterPersonResponse.From, ct);
    }
}

