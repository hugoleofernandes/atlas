using Atlas.API.Errors;
using Atlas.API.Models.Roles;
using Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;
using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.BuildingBlocks.AspNetCore.Security.Authorization;
using Atlas.Identity.Application.Tenants.Commands.CreateRole;
using Atlas.Identity.Application.Tenants.Commands.RemoveRole;
using Atlas.Identity.Application.Tenants.Commands.UpdateRole;
using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.Identity.Application.Tenants.Queries.GetRoleById;
using Atlas.Identity.Application.Tenants.Queries.ListRoles;
using Atlas.Identity.Domain.Permissions;
using Atlas.SharedKernel.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dtos = Atlas.Identity.Application.Tenants.Queries.Dtos;

namespace Atlas.API.Controllers.Identity;

[ApiController]
[Route("tenants/roles")]
[Authorize]
public sealed class RolesController(
    ICreateRoleCommandHandler createRoleHandler,
    IUpdateRoleCommandHandler updateRoleHandler,
    IRemoveRoleCommandHandler removeRoleHandler,
    IListRolesQueryHandler listRolesQueryHandler,
    IGetRoleByIdQueryHandler getRoleByIdQueryHandler,
    IHandlerInvoker invoker,
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
        page     = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query  = new ListRolesQuery(page, pageSize, includeInactive);
        var result = await invoker.InvokeAsync(listRolesQueryHandler, query, ct);
        return Ok(result.Value);
    }

    /// <summary>
    /// Returns a single role by id. Returns both active and inactive roles.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(PermissionCatalog.Tenant.ManageRoles)]
    [ProducesResponseType(typeof(Dtos.RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query  = new GetRoleByIdQuery(id);
        var result = await invoker.InvokeAsync(getRoleByIdQueryHandler, query, ct);

        if (result.Value is null)
            return NotFound();

        return Ok(result.Value);
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
        var cmd    = new CreateRoleCommand(request.Name, request.PermissionCodes);
        var result = await invoker.InvokeAsync(createRoleHandler, cmd, ct);

        if (!result.IsSuccess)
            return ErrorResult(result.ErrorDefinition!);

        var value = result.Value!;
        return CreatedAtAction(
            nameof(Create),
            new CreateRoleResponse(value.RoleId, value.Name, value.PermissionCodes));
    }

    /// <summary>
    /// Updates the name and permissions of a custom role.
    /// System roles cannot be updated.
    /// Role name must be unique within the tenant (including inactive roles).
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(PermissionCatalog.Tenant.ManageRoles)]
    [ProducesResponseType(typeof(UpdateRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateRoleRequest request,
        CancellationToken ct)
    {
        var cmd    = new UpdateRoleCommand(id, request.Name, request.PermissionCodes);
        var result = await invoker.InvokeAsync(updateRoleHandler, cmd, ct);

        if (!result.IsSuccess)
            return ErrorResult(result.ErrorDefinition!);

        var value = result.Value!;
        return Ok(new UpdateRoleResponse(value.RoleId, value.Name, value.PermissionCodes));
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
        var cmd    = new RemoveRoleCommand(id);
        var result = await invoker.InvokeAsync(removeRoleHandler, cmd, ct);

        if (!result.IsSuccess)
            return ErrorResult(result.ErrorDefinition!);

        return NoContent();
    }
}
