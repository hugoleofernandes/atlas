using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Roles.LookupRoles;
using Atlas.SharedDomain.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.API.Endpoints.Roles.LookupRoles;

/// <summary>
/// Returns a lightweight id+name list of active roles for populating dropdowns.
/// </summary>
public sealed class LookupRolesEndpoint(ILookupRolesQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<EmptyRequest, IReadOnlyList<RoleLookupDto>>
{
    public override void Configure()
    {
        Get("roles/lookup");
        Policies($"permission:{IdentityModulePermissions.Tenant.Roles.Read}");
        Description(d => d.Produces<IReadOnlyList<RoleLookupDto>>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new LookupRolesQuery(), ct);
        await OkFromResultAsync(result, ct);
    }
}
