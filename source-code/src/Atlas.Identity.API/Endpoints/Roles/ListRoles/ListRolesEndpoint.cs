using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Roles.ListRoles;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.API.Endpoints.Roles.ListRoles;

public sealed class ListRolesEndpoint(IListRolesQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<ListRolesRequest, IReadOnlyList<RoleDto>>
{
    public override void Configure()
    {
        Get("roles");
        Policies($"permission:{IdentityModulePermissions.Tenant.Roles.Read}");
        Description(d => d.Produces<IReadOnlyList<RoleDto>>());
    }

    public override async Task HandleAsync(ListRolesRequest req, CancellationToken ct)
    {
        var query = new ListRolesQuery(req.IncludeInactive);
        var result = await invoker.InvokeAsync(handler, query, ct);
        await OkFromResultAsync(result, ct);
    }
}
