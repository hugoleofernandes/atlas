using Atlas.API.Errors;
using Atlas.API.Models.Roles;
using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.BuildingBlocks.AspNetCore.Security.Authorization;
using Atlas.Identity.Application.Tenants.Commands.CreateRole;
using Atlas.Identity.Application.Tenants.Workflows.CreateRole;
using Atlas.Identity.Domain.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.API.Controllers.Identity;

[ApiController]
[Route("tenants/roles")]
[Authorize]
public sealed class RolesController(
    ICreateRoleWorkflow createRoleWorkflow,
    ErrorMessageLocalizer errorLocalizer
) : AtlasControllerBase(errorLocalizer)
{
    /// <summary>
    /// Creates a new custom role for the authenticated user's tenant.
    /// System roles (root, admin, member) cannot be created via this endpoint.
    /// </summary>
    [HttpPost]
    [HasPermission(PermissionCatalog.Tenant.ManageRoles)]
    [ProducesResponseType(typeof(CreateRoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoleRequest request,
        CancellationToken ct)
    {
        var cmd = new Command(request.Name, request.PermissionCodes);

        var result = await createRoleWorkflow.ExecuteAsync(cmd, ct);

        if (!result.IsSuccess)
            return ErrorResult(result.ErrorDefinition!);

        var value = result.Value!;

        return CreatedAtAction(
            nameof(Create),
            new CreateRoleResponse(value.RoleId, value.Name, value.PermissionCodes));
    }
}
