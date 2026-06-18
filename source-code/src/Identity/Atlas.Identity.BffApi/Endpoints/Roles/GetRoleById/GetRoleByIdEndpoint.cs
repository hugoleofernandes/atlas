using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Roles.GetRoleById;
using Atlas.Identity.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Roles.GetRoleById;

/// <summary>
/// Returns a single role by id. Returns both active and inactive roles.
/// </summary>
public sealed class GetRoleByIdEndpoint(IGetRoleByIdQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<GetRoleByIdRequest, GetRoleByIdDto?>
{
    public override void Configure()
    {
        Get("bff/v1/identity/roles/{id}");
        Policies($"permission:{IdentityModulePermissions.Roles.Read}");
        Description(d => d.Produces<GetRoleByIdDto>());
    }

    public override async Task HandleAsync(GetRoleByIdRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new GetRoleByIdQuery(req.Id), ct);
        await OkFromResultAsync(result, ct);
    }
}
