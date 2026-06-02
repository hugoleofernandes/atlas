using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Permissions.ListPermissions;
using Atlas.SharedDomain.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.API.Endpoints.Permissions.ListPermissions;

/// <summary>
/// Returns all assignable permissions grouped by resource, with localized labels.
/// Used by the frontend to render permission selectors when creating or editing roles.
/// </summary>
public sealed class ListPermissionsEndpoint(
    IListPermissionsQueryHandler handler,
    PermissionLabelLocalizer labelLocalizer,
    IHandlerInvoker invoker
) : AtlasEndpoint<EmptyRequest, IReadOnlyList<PermissionModuleResponse>>
{
    public override void Configure()
    {
        Get("permissions");
        Policies($"permission:{IdentityModulePermissions.Tenant.Roles.Read}");
        Description(d => d.Produces<IReadOnlyList<PermissionModuleResponse>>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var query = new ListPermissionsQuery();
        var result = await invoker.InvokeAsync(handler, query, ct);

        await OkFromResultAsync(
            result,
            modules =>
                modules
                    .Select(module => new PermissionModuleResponse(
                        ModuleId: module.ModuleId,
                        ModuleName: module.ModuleName,
                        Groups: module.Groups
                            .Select(g => new PermissionGroupResponse(
                                Manage: new PermissionItemResponse(g.Manage, labelLocalizer.Localize(g.Manage)),
                                Granular: g.Granular.Select(code => new PermissionItemResponse(
                                        code,
                                        labelLocalizer.Localize(code)
                                    ))
                                    .ToList()
                            ))
                            .ToList()
                    ))
                    .ToList(),
            ct
        );
    }
}
