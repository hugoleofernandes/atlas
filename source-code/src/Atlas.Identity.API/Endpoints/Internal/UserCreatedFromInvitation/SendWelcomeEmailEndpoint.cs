using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.BuildingBlocks.AspNetCore.Security.InternalApi;
using Atlas.Identity.Application.Commands.SendWelcomeEmail;
using Atlas.Integration.Contracts.Tenants;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.API.Endpoints.Internal.UserCreatedFromInvitation;

public sealed class SendWelcomeEmailEndpoint(
    ISendWelcomeEmailCommandHandler handler,
    IHandlerInvoker invoker)
    : InternalAtlasEndpoint<UserCreatedFromInvitationIntegrationEvent, EmptyResponse>
{
    public override void Configure()
    {
        Post("internal/identity/events/user-created-from-invitation/send-welcome-email");
        Policies(InternalApiKeyDefaults.PolicyName);
        Description(d => d.Produces(204));
    }

    public override async Task HandleAsync(UserCreatedFromInvitationIntegrationEvent req, CancellationToken ct)
    {
        if (!await HydrateOutboxContextAsync(ct))
            return;

        var command = new SendWelcomeEmailCommand(req.TenantId, req.UserId, req.Email);
        var result = await invoker.InvokeAsync(handler, command, ct);
        await UpdatedNoContentFromResultAsync(result, ct);
    }
}
