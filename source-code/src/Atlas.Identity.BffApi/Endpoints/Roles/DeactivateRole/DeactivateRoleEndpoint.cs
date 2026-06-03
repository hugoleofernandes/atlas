using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Commands.DeactivateRole;
using Atlas.Identity.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Roles.DeactivateRole;

/// <summary>
/// Suspends a custom role without deleting it.
/// Users assigned to this role stop receiving permissions on future sessions.
/// System roles cannot be changed.
/// </summary>
public sealed class DeactivateRoleEndpoint(IDeactivateRoleCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<DeactivateRoleRequest, EmptyResponse>
{
    public override void Configure()
    {
        Patch("bff/identity/roles/{id}/deactivate");
        Policies($"permission:{ModulePermissions.Roles.Update}");
        Description(d => d.Produces(204));
    }

    public override async Task HandleAsync(DeactivateRoleRequest req, CancellationToken ct)
    {
        var cmd = new DeactivateRoleCommand(req.Id);
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await UpdatedNoContentFromResultAsync(result, ct);
    }
}
