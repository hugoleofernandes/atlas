using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Commands.CreateRole;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.API.Endpoints.Roles.CreateRole;

/// <summary>
/// Creates a new custom role for the authenticated user's tenant.
/// System roles (root, admin, member) cannot be created via this endpoint.
/// </summary>
public sealed class CreateRoleEndpoint(ICreateRoleCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<CreateRoleRequest, CreateRoleResponse>
{
    public override void Configure()
    {
        Post("roles");
        Policies($"permission:{IdentityModulePermissions.Tenant.Roles.Create}");
        Description(d => d.Produces<CreateRoleResponse>(201));
    }

    public override async Task HandleAsync(CreateRoleRequest req, CancellationToken ct)
    {
        var cmd = new CreateRoleCommand(req.Name, req.PermissionCodes);
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await CreatedFromResultAsync(result, CreateRoleResponse.From, ct);
    }
}
