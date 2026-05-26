using Atlas.API.Errors;
using Atlas.API.Models.Invitations;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.BuildingBlocks.AspNetCore.Security.Authorization;
using Atlas.Identity.Application.Tenants.Commands.InviteUser;
using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.Identity.Application.Tenants.Queries.ListInvitations;
using Atlas.Identity.Domain.Permissions;
using Atlas.SharedKernel.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.API.Controllers.Identity;

[ApiController]
[Route("tenants/invitations")]
[Authorize]
public sealed class InvitationController(
    IInviteUserCommandHandler inviteUserHandler,
    IListInvitationsQueryHandler listInvitationsQueryHandler,
    IHandlerInvoker invoker,
    ErrorMessageLocalizer errorLocalizer,
    IHttpResultMapper resultMapper
) : AtlasControllerBase(errorLocalizer, resultMapper)
{
    /// <summary>
    /// Lists all invitations for the authenticated user's tenant, paginated.
    /// </summary>
    [HttpGet]
    [HasPermission(PermissionCatalog.Tenant.InviteUser)]
    [ProducesResponseType(typeof(PagedResult<InvitationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page     = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query  = new ListInvitationsQuery(page, pageSize);
        var result = await invoker.InvokeAsync(listInvitationsQueryHandler, query, ct);

        return OkFromResult(result);
    }

    /// <summary>
    /// Invites a new user to the authenticated user's tenant.
    /// The tenant is resolved from the session cookie — not from the URL.
    /// </summary>
    [HttpPost]
    [HasPermission(PermissionCatalog.Tenant.InviteUser)]
    [ProducesResponseType(typeof(InviteUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Invite(
        [FromBody] InviteUserRequest request,
        CancellationToken ct)
    {
        var cmd    = new InviteUserCommand(request.Email, request.RoleId);
        var result = await invoker.InvokeAsync(inviteUserHandler, cmd, ct);

        return CreatedFromResult<InviteUserOutput, InviteUserResponse>(result);
    }
}
