using Microsoft.AspNetCore.Http;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Commands.UpdateRole;

namespace Atlas.Identity.API.Endpoints.Tenants._Roles.UpdateRole;

/// <summary>
/// Updates the name and permissions of a custom role.
/// System roles cannot be updated.
/// Role name must be unique within the tenant (including inactive roles).
/// </summary>
public sealed class UpdateRoleEndpoint(
    IUpdateRoleCommandHandler handler,
    IHandlerInvoker           invoker
) : AtlasEndpoint<UpdateRoleRequest, UpdateRoleResponse>
{
    public override void Configure()
    {
        Put("identity/roles/{id}");
        Policies($"permission:{IdentityModulePermissions.Tenant.Roles.Update}");
        Description(d => d.Produces<UpdateRoleResponse>());
    }

    public override async Task HandleAsync(UpdateRoleRequest req, CancellationToken ct)
    {
        var cmd    = new UpdateRoleCommand(req.Id, req.Name, req.PermissionCodes);
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await UpdatedFromResultAsync(result, UpdateRoleResponse.From, ct);
    }
}
