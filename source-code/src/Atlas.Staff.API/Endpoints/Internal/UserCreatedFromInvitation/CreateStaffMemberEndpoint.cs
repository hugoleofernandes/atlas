using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Integration.Contracts.Tenants;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Application.StaffMembers.Commands.CreateFromInvitation;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Staff.API.Endpoints.Internal.UserCreatedFromInvitation;

public sealed class CreateStaffMemberEndpoint(
    ICreateStaffMemberFromInvitationCommandHandler handler,
    IHandlerInvoker invoker)
    : InternalAtlasEndpoint<UserCreatedFromInvitationIntegrationEvent, EmptyResponse>
{
    public override void Configure()
    {
        Post("internal/staff/events/user-created-from-invitation/create-staff-member");
        AllowAnonymous();
        Description(d => d.Produces(204));
    }

    public override async Task HandleAsync(UserCreatedFromInvitationIntegrationEvent req, CancellationToken ct)
    {
        if (!await AuthorizeAndHydrateOutboxContextAsync(ct))
            return;

        var command = new CreateStaffMemberFromInvitationCommand(req.TenantId, req.UserId, req.Email, req.Role);
        var result = await invoker.InvokeAsync(handler, command, ct);
        await UpdatedNoContentFromResultAsync(result, ct);
    }
}
