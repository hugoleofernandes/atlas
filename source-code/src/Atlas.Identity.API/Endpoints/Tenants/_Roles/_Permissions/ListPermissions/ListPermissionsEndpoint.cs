using Microsoft.AspNetCore.Http;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.Identity.Application.Aggregates.Tenants._Roles._Permissions.Handlers.Queries.ListPermissions;

namespace Atlas.Identity.API.Endpoints.Tenants._Roles._Permissions.ListPermissions;

/// <summary>
/// Returns all assignable permissions grouped by resource, with localized labels.
/// Used by the frontend to render permission selectors when creating or editing roles.
/// </summary>
public sealed class ListPermissionsEndpoint(
    IListPermissionsQueryHandler handler,
    PermissionLabelLocalizer     labelLocalizer,
    IHandlerInvoker              invoker
) : AtlasEndpoint<EmptyRequest, IReadOnlyList<PermissionGroupResponse>>
{
    public override void Configure()
    {
        Get("identity/roles/permissions");
        Policies($"permission:{IdentityModulePermissions.Tenant.Roles.Read}");
        Description(d => d.Produces<IReadOnlyList<PermissionGroupResponse>>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var query  = new ListPermissionsQuery();
        var result = await invoker.InvokeAsync(handler, query, ct);

        await OkFromResultAsync(result, groups => groups
            .Select(g => new PermissionGroupResponse(
                Manage:   new PermissionItemResponse(g.Manage,   labelLocalizer.Localize(g.Manage)),
                Granular: g.Granular
                           .Select(code => new PermissionItemResponse(code, labelLocalizer.Localize(code)))
                           .ToList()))
            .ToList<PermissionGroupResponse>(), ct);
    }
}
