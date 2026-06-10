using Atlas.Identity.Application.Commands.SendInvitationEmail;
using Atlas.Identity.Contracts.IntegrationEvents.Users;
using Atlas.Outbox.Application.Targets;
using Atlas.Outbox.Domain.Targets.Names;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;

namespace Atlas.Outbox.Targets.Identity.UserInvited;

public sealed class SendInvitationEmailTargetHandler(
    ISendInvitationEmailCommandHandler handler,
    IHandlerInvoker invoker,
    IIdempotencyContextSetter idempotencyContextSetter
) : OutboxTargetHandler<UserInvitedIntegrationEvent, SendInvitationEmailCommand>(handler, invoker, idempotencyContextSetter)
{
    public override string Name => IdentityTargetNames.IdentitySendInvitationEmail;

    protected override SendInvitationEmailCommand MapToCommand(UserInvitedIntegrationEvent @event) =>
        new(@event.TenantId, @event.Email);
}
