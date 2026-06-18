using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Users.GetUserById;
using Atlas.Identity.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Users.GetUserById;

/// <summary>
/// Returns a single user by id. Returns both active and inactive users.
/// </summary>
public sealed class GetUserByIdEndpoint(IGetUserByIdQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<GetUserByIdRequest, GetUserByIdDto?>
{
    public override void Configure()
    {
        Get("bff/v1/identity/users/{id}");
        Policies($"permission:{IdentityModulePermissions.Users.Read}");
        Description(d => d.Produces<GetUserByIdDto>());
    }

    public override async Task HandleAsync(GetUserByIdRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new GetUserByIdQuery(req.Id), ct);
        await OkFromResultAsync(result, ct);
    }
}
