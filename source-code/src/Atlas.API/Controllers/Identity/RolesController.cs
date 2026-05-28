using Atlas.API.Errors;
using Atlas.API.Models.Roles;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.BuildingBlocks.AspNetCore.Security.Authorization;
using Atlas.Identity.Application.Tenants.Commands.CreateRole;
using Atlas.Identity.Application.Tenants.Commands.RemoveRole;
using Atlas.Identity.Application.Tenants.Commands.UpdateRole;
using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.Identity.Application.Tenants.Queries.GetRoleById;
using Atlas.Identity.Application.Tenants.Queries.ListRoles;
using Atlas.Identity.Application.Tenants.Queries.ListPermissions;
using Atlas.Identity.Application.Tenants.Queries.LookupRoles;
using Atlas.SharedKernel.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dtos = Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.Identity.Domain.Entities.Tenants.Roles.Permissions;

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
    ILookupRolesQueryHandler lookupRolesQueryHandler,
    IListPermissionsQueryHandler listPermissionsQueryHandler,
    PermissionLabelLocalizer permissionLabelLocalizer,
    IHandlerInvoker invoker,
    ErrorMessageLocalizer errorLocalizer,
    IHttpResultMapper resultMapper
) : AtlasControllerBase(errorLocalizer, resultMapper)
{
    /// <summary>
    /// Returns all assignable permissions grouped by resource, with localized labels.
    /// Used by the frontend to render permission selectors when creating or editing roles.
    /// </summary>
    [HttpGet("permissions")]
    [HasPermission(IdentityPermissions.Tenant.Roles.Read)]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionGroupResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPermissions(CancellationToken ct)
    {
        var query  = new ListPermissionsQuery();
        var result = await invoker.InvokeAsync(listPermissionsQueryHandler, query, ct);

        var response = result.Value!
            .Select(g => new PermissionGroupResponse(
                Manage:   new PermissionItemResponse(g.Manage,   permissionLabelLocalizer.Localize(g.Manage)),
                Granular: g.Granular
                           .Select(code => new PermissionItemResponse(code, permissionLabelLocalizer.Localize(code)))
                           .ToList()))
            .ToList();

        return Ok(response);
    }

    /// <summary>
    /// Returns a lightweight id+name list of active roles for populating dropdowns.
    /// Requires InviteUser permission so invitation editors can use it without ManageRoles.
    /// </summary>
    [HttpGet("lookup")]
    [HasPermission(IdentityPermissions.Tenant.Roles.Read)]
    [ProducesResponseType(typeof(IReadOnlyList<RoleLookupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Lookup(CancellationToken ct)
    {
        var query  = new LookupRolesQuery();
        var result = await invoker.InvokeAsync(lookupRolesQueryHandler, query, ct);
        return OkFromResult(result);
    }

    /// <summary>
    /// Lists all roles for the authenticated user's tenant, paginated.
    /// </summary>
    [HttpGet]
    [HasPermission(IdentityPermissions.Tenant.Roles.Read)]
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
        return OkFromResult(result);
    }

    /// <summary>
    /// Returns a single role by id. Returns both active and inactive roles.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(IdentityPermissions.Tenant.Roles.Read)]
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
    [HasPermission(IdentityPermissions.Tenant.Roles.Create)]
    [ProducesResponseType(typeof(CreateRoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoleRequest request,
        CancellationToken ct)
    {
        var cmd    = new CreateRoleCommand(request.Name, request.PermissionCodes);
        var result = await invoker.InvokeAsync(createRoleHandler, cmd, ct);

        return CreatedAtActionFromResult<CreateRoleOutput, CreateRoleResponse>(
            result,
            nameof(GetById),
            value => new { id = value.RoleId });
    }

    /// <summary>
    /// Updates the name and permissions of a custom role.
    /// System roles cannot be updated.
    /// Role name must be unique within the tenant (including inactive roles).
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(IdentityPermissions.Tenant.Roles.Update)]
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

        return UpdatedFromResult<UpdateRoleOutput, UpdateRoleResponse>(result);
    }

    /// <summary>
    /// Removes a custom role from the tenant.
    /// Hard delete if the role was never assigned; soft delete (inactive) if it has historical references.
    /// System roles cannot be removed.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(IdentityPermissions.Tenant.Roles.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Remove(Guid id, CancellationToken ct)
    {
        var cmd    = new RemoveRoleCommand(id);
        var result = await invoker.InvokeAsync(removeRoleHandler, cmd, ct);

        return DeletedFromResult(result);
    }
}
