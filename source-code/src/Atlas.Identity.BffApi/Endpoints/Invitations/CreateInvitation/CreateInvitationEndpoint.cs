using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Commands.InviteUser;
using Atlas.Identity.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Invitations.CreateInvitation;

/// <summary>
/// Creates an invitation for a new user to join the authenticated user's tenant.
/// The tenant is resolved from the session cookie â€” not from the URL.
/// </summary>
public sealed class CreateInvitationEndpoint(IInviteUserCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<CreateInvitationRequest, CreateInvitationResponse>
{
    public override void Configure()
    {
        Post("bff/identity/invitations");
        Policies($"permission:{ModulePermissions.Invitations.Create}");
        Description(d => d.Produces<CreateInvitationResponse>(201));
    }

    public override async Task HandleAsync(CreateInvitationRequest req, CancellationToken ct)
    {
        var cmd = new InviteUserCommand(req.Email, req.RoleId);
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await CreatedFromResultAsync(result, CreateInvitationResponse.From, ct);
    }
}
