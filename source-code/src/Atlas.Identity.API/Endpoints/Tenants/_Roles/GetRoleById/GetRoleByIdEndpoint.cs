using Microsoft.AspNetCore.Http;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.Identity.Application.Queries.Roles.ListRoles;
using Atlas.Identity.Application.Queries.Roles.GetRoleById;

namespace Atlas.Identity.API.Endpoints.Tenants._Roles.GetRoleById;

/// <summary>
/// Returns a single role by id. Returns both active and inactive roles.
/// </summary>
public sealed class GetRoleByIdEndpoint(
    IGetRoleByIdQueryHandler handler,
    IHandlerInvoker          invoker
) : AtlasEndpoint<GetRoleByIdRequest, RoleDto>
{
    public override void Configure()
    {
        Get("identity/roles/{id}");
        Policies($"permission:{IdentityModulePermissions.Tenant.Roles.Read}");
        Description(d => d.Produces<RoleDto>());
    }

    public override async Task HandleAsync(GetRoleByIdRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new GetRoleByIdQuery(req.Id), ct);

        if (!result.IsSuccess)
        {
            var localizer  = Resolve<IErrorMessageLocalizer>();
            var statusCode = result.ErrorDefinition!.Category.ToHttpStatus();
            await Send.ResultAsync(Results.Problem(
                title:      localizer.Localize(result.ErrorDefinition!),
                detail:     result.ErrorDefinition!.FallbackMessage,
                statusCode: statusCode));
            return;
        }

        if (result.Value is null)
        {
            await Send.ResultAsync(Results.NotFound());
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
