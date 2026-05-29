using Microsoft.AspNetCore.Http;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.Identity.Application.Queries.Roles.ListRoles;

namespace Atlas.Identity.API.Endpoints.Tenants._Roles.ListRoles;

/// <summary>
/// Lists all roles for the authenticated user's tenant, paginated.
/// </summary>
public sealed class ListRolesEndpoint(
    IListRolesQueryHandler handler,
    IHandlerInvoker        invoker
) : AtlasEndpoint<ListRolesRequest, PagedResult<RoleDto>>
{
    public override void Configure()
    {
        Get("identity/roles");
        Policies($"permission:{IdentityModulePermissions.Tenant.Roles.Read}");
        Description(d => d.Produces<PagedResult<RoleDto>>());
    }

    public override async Task HandleAsync(ListRolesRequest req, CancellationToken ct)
    {
        var page     = Math.Max(1, req.Page);
        var pageSize = Math.Clamp(req.PageSize, 1, 100);

        var query  = new ListRolesQuery(page, pageSize, req.IncludeInactive);
        var result = await invoker.InvokeAsync(handler, query, ct);
        await OkFromResultAsync(result, ct);
    }
}
