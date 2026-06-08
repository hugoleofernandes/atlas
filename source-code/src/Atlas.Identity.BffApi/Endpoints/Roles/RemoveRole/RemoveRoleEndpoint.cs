using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Commands.RemoveRole;
using Atlas.Identity.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Roles.RemoveRole;

/// <summary>
/// Removes a custom role from the tenant.
/// Hard delete if the role was never assigned; soft delete (inactive) if it has historical references.
/// System roles cannot be removed.
/// </summary>
public sealed class RemoveRoleEndpoint(IRemoveRoleCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<RemoveRoleRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("bff/v1/identity/roles/{id}");
        Policies($"permission:{IdentityModulePermissions.Roles.Delete}");
        Description(d => d.Produces(204));
    }

    public override async Task HandleAsync(RemoveRoleRequest req, CancellationToken ct)
    {
        var cmd = new RemoveRoleCommand(req.Id);
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await DeletedFromResultAsync(result, ct);
    }
}
