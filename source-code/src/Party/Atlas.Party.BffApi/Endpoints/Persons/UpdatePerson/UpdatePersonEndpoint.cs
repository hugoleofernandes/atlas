using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Commands.UpdatePerson;
using Atlas.Party.BffApi.Endpoints.Shared;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Persons.UpdatePerson;

/// <summary>
/// Updates mutable personal details of an existing person. Does not change TaxNumber or TenantId.
/// </summary>
public sealed class UpdatePersonEndpoint(IUpdatePersonCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<UpdatePersonRequest, UpdatePersonResponse>
{
    public override void Configure()
    {
        Put("bff/v1/party/persons/{id}");
        Policies($"permission:{PartyModulePermissions.Person.Update.Code}");
        Description(d => d.Produces<UpdatePersonResponse>());
    }

    public override async Task HandleAsync(UpdatePersonRequest req, CancellationToken ct)
    {
        var cmd = new UpdatePersonCommand(
            req.Id,
            req.FirstName,
            req.LastName,
            req.MiddleName,
            req.BirthDate,
            req.Gender,
            AddressRequestMapper.ToAddressInputs(req.Addresses),
            ContactRequestMapper.ToContactInputs(req.Contacts)
        );
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await UpdatedFromResultAsync(result, UpdatePersonResponse.From, ct);
    }
}

