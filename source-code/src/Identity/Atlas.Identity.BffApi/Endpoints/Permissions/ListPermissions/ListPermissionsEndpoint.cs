using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Queries.Permissions.ListPermissions;
using Atlas.Identity.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Permissions.ListPermissions;

/// <summary>
/// Returns permissions from the persisted catalog, with optional active-state filtering and localized labels.
/// </summary>
public sealed class ListPermissionsEndpoint(
    IListPermissionsQueryHandler handler,
    PermissionLabelLocalizer labelLocalizer,
    IHandlerInvoker invoker
) : AtlasEndpoint<ListPermissionsRequest, IReadOnlyList<PermissionItemResponse>>
{
    public override void Configure()
    {
        Get("bff/v1/identity/permissions");
        Policies($"permission:{IdentityModulePermissions.Roles.Read}");
        Description(d => d.Produces<IReadOnlyList<PermissionItemResponse>>());
    }

    public override async Task HandleAsync(ListPermissionsRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new ListPermissionsQuery(req.IsActive), ct);

        var response = result.Map(x => PermissionItemResponse.FromList(x, labelLocalizer));

        await OkFromResultAsync(response, ct);
    }
}
