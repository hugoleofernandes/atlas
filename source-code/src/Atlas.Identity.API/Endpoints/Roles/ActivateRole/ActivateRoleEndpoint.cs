using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Commands.ActivateRole;
using Atlas.SharedDomain.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.API.Endpoints.Roles.ActivateRole;

/// <summary>
/// Re-enables a custom role so it can grant permissions again on future sessions.
/// System roles cannot be changed.
/// </summary>
public sealed class ActivateRoleEndpoint(IActivateRoleCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<ActivateRoleRequest, EmptyResponse>
{
    public override void Configure()
    {
        Patch("roles/{id}/activate");
        Policies($"permission:{IdentityModulePermissions.Tenant.Roles.Update}");
        Description(d => d.Produces(204));
    }

    public override async Task HandleAsync(ActivateRoleRequest req, CancellationToken ct)
    {
        var cmd = new ActivateRoleCommand(req.Id);
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await UpdatedNoContentFromResultAsync(result, ct);
    }
}
