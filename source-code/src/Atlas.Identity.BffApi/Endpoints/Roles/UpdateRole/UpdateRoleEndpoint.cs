using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Commands.UpdateRole;
using Atlas.Identity.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Roles.UpdateRole;

/// <summary>
/// Updates the name and permissions of a custom role.
/// System roles cannot be updated.
/// Role name must be unique within the tenant (including inactive roles).
/// </summary>
public sealed class UpdateRoleEndpoint(IUpdateRoleCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<UpdateRoleRequest, UpdateRoleResponse>
{
    public override void Configure()
    {
        Put("bff/identity/roles/{id}");
        Policies($"permission:{ModulePermissions.Roles.Update}");
        Description(d => d.Produces<UpdateRoleResponse>());
    }

    public override async Task HandleAsync(UpdateRoleRequest req, CancellationToken ct)
    {
        var cmd = new UpdateRoleCommand(req.Id, req.Name, req.PermissionCodes);
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await UpdatedFromResultAsync(result, UpdateRoleResponse.From, ct);
    }
}
