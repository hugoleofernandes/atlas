using Atlas.API.Errors;
using Atlas.API.Models.Roles;
using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.BuildingBlocks.AspNetCore.Security.Authorization;
using Atlas.Identity.Application.Tenants.Commands.CreateRole;
using Atlas.Identity.Application.Tenants.Queries.ListRoles;
using Atlas.Identity.Application.Tenants.Workflows.CreateRole;
using Atlas.Identity.Application.Tenants.Workflows.RemoveRole;
using Atlas.Identity.Domain.Permissions;
using Atlas.SharedKernel.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemoveRole = Atlas.Identity.Application.Tenants.Commands.RemoveRole;

namespace Atlas.API.Controllers.Identity;

[ApiController]
[Route("tenants/roles")]
[Authorize]
public sealed class RolesController(
    ICreateRoleWorkflow createRoleWorkflow,
    IRemoveRoleWorkflow removeRoleWorkflow,
    IListRolesQueryHandler listRolesQueryHandler,
    ErrorMessageLocalizer errorLocalizer
) : AtlasControllerBase(errorLocalizer)
{
    /// <summary>
    /// Lists all roles for the authenticated user's tenant, paginated.
    /// </summary>
    [HttpGet]
    [HasPermission(PermissionCatalog.Tenant.ManageRoles)]
    [ProducesResponseType(typeof(PagedResult<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var result = await listRolesQueryHandler.ExecuteAsync(new Query(page, pageSize, includeInactive), ct);
        return Ok(result);
    }

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

    /// <summary>
    /// Removes a custom role from the tenant.
    /// Hard delete if the role was never assigned; soft delete (inactive) if it has historical references.
    /// System roles cannot be removed.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionCatalog.Tenant.ManageRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Remove(Guid id, CancellationToken ct)
    {
        var cmd = new RemoveRole.Command(id);
        var result = await removeRoleWorkflow.ExecuteAsync(cmd, ct);

        if (!result.IsSuccess)
            return ErrorResult(result.ErrorDefinition!);

        return NoContent();
    }
}
