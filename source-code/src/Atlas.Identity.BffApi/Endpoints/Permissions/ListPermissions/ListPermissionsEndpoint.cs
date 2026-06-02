using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Permissions.ListPermissions;
using Atlas.SharedDomain.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Permissions.ListPermissions;

/// <summary>
/// Returns all assignable permissions grouped by resource, with localized labels.
/// Used by the frontend to render permission selectors when creating or editing roles.
/// </summary>
public sealed class ListPermissionsEndpoint(
    IListPermissionsQueryHandler handler,
    PermissionLabelLocalizer labelLocalizer,
    IHandlerInvoker invoker
) : AtlasEndpoint<EmptyRequest, IReadOnlyList<PermissionItemResponse>>
{
    public override void Configure()
    {
        Get("bff/identity/permissions");
        Policies($"permission:{IdentityModulePermissions.Tenant.Roles.Read}");
        Description(d => d.Produces<IReadOnlyList<PermissionItemResponse>>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new ListPermissionsQuery(), ct);

        await OkFromResultAsync(
            result,
            x => x.Select(i => PermissionItemResponse.From(i, labelLocalizer)).ToList(),
            ct
        );
    }
}
