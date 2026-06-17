using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Queries.Persons;
using Atlas.Party.Application.Queries.Persons.GetPersonById;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Persons.GetPersonById;

/// <summary>
/// Returns a single person by id. Returns both active and inactive persons.
/// </summary>
public sealed class GetPersonByIdEndpoint(IGetPersonByIdQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<GetPersonByIdRequest, PersonDto>
{
    public override void Configure()
    {
        Get("bff/v1/party/persons/{id}");
        Policies($"permission:{PartyModulePermissions.Person.Read.Code}");
        Description(d => d.Produces<PersonDto>());
    }

    public override async Task HandleAsync(GetPersonByIdRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new GetPersonByIdQuery(req.Id), ct);
        await OkFromResultAsync(result, ct);
    }
}

