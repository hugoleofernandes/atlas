using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Roles.ListRoles;
using Atlas.Identity.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Roles.ListRoles;

public sealed class ListRolesEndpoint(IListRolesQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<ListRolesRequest, IReadOnlyList<ListRolesDto>>
{
    public override void Configure()
    {
        Get("bff/v1/identity/roles");
        Policies($"permission:{IdentityModulePermissions.Roles.Read}");
        Description(d => d.Produces<IReadOnlyList<ListRolesDto>>());
    }

    public override async Task HandleAsync(ListRolesRequest req, CancellationToken ct)
    {
        var query = new ListRolesQuery(req.IsActive);
        var result = await invoker.InvokeAsync(handler, query, ct);
        await OkFromResultAsync(result, ct);
    }
}
