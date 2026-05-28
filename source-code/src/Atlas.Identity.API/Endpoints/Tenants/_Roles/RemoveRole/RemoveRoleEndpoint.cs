using Microsoft.AspNetCore.Http;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Commands.RemoveRole;

namespace Atlas.Identity.API.Endpoints.Tenants._Roles.RemoveRole;

/// <summary>
/// Removes a custom role from the tenant.
/// Hard delete if the role was never assigned; soft delete (inactive) if it has historical references.
/// System roles cannot be removed.
/// </summary>
public sealed class RemoveRoleEndpoint(
    IRemoveRoleCommandHandler handler,
    IHandlerInvoker           invoker
) : AtlasEndpoint<RemoveRoleRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("identity/roles/{id}");
        Policies($"permission:{IdentityModulePermissions.Tenant.Roles.Delete}");
        Description(d => d.Produces(204));
    }

    public override async Task HandleAsync(RemoveRoleRequest req, CancellationToken ct)
    {
        var cmd    = new RemoveRoleCommand(req.Id);
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await DeletedFromResultAsync(result, ct);
    }
}
