using Atlas.Identity.Application.Commands.SendWelcomeEmail;
using Atlas.Identity.Contracts.IntegrationEvents.Users;
using Atlas.Outbox.Application.Targets;
using Atlas.Outbox.Domain.Targets.Names;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;

namespace Atlas.Outbox.Targets.Identity.UserCreatedFromInvitation;

public sealed class SendWelcomeEmailTargetHandler(
    ISendWelcomeEmailCommandHandler handler,
    IHandlerInvoker invoker,
    IIdempotencyContextSetter idempotencyContextSetter
) : OutboxTargetHandler<UserCreatedFromInvitationIntegrationEvent, SendWelcomeEmailCommand>(handler, invoker, idempotencyContextSetter)
{
    public override string Name => IdentityTargetNames.IdentitySendWelcomeEmail;

    protected override SendWelcomeEmailCommand MapToCommand(UserCreatedFromInvitationIntegrationEvent @event) =>
        new(@event.TenantId, @event.UserId, @event.Email);
}
