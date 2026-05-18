using Atlas.API.Errors;
using Atlas.API.Models.Invitations;
using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.BuildingBlocks.AspNetCore.Security.Authorization;
using Atlas.Identity.Application.Tenants.Commands.InviteUser;
using Atlas.Identity.Application.Tenants.Workflows.InviteUser;
using Atlas.Identity.Domain.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.API.Controllers.Identity;

[ApiController]
[Route("tenants/invitations")]
[Authorize]
public sealed class InvitationController(
    IInviteUserWorkflow inviteUserWorkflow,
    ErrorMessageLocalizer errorLocalizer
) : AtlasControllerBase(errorLocalizer)
{
    /// <summary>
    /// Invites a new user to the authenticated user's tenant.
    /// The tenant is resolved from the session cookie â€” not from the URL.
    /// </summary>
    [HttpPost]
    [HasPermission(PermissionCatalog.Tenant.InviteUser)]
    [ProducesResponseType(typeof(InviteUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Invite(
        [FromBody] InviteUserRequest request,
        CancellationToken ct)
    {
        var cmd = new Command(request.Email, request.RoleId);

        var result = await inviteUserWorkflow.ExecuteAsync(cmd, ct);

        if (!result.IsSuccess)
            return ErrorResult(result.ErrorDefinition!);

        var value = result.Value!;

        return CreatedAtAction(
            nameof(Invite),
            new InviteUserResponse(
                value.InvitationId,
                value.Email,
                value.RoleId,
                value.RoleName,
                value.ExpiresAt));
    }
}

public sealed record InviteUserResponse(
    Guid InvitationId,
    string Email,
    Guid RoleId,
    string RoleName,
    DateTime ExpiresAt
);

