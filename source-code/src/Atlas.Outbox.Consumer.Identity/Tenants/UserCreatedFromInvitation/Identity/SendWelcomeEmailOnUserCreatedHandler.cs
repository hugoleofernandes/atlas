using Atlas.Identity.Application.Tenants.Commands.SendWelcomeEmail;
using Atlas.Integration.Contracts.Tenants;
using Atlas.SharedKernel.Application.IntegrationEvents;

namespace Atlas.Outbox.Consumer.Identity.Tenants.UserCreatedFromInvitation.Identity;

/// <summary>
/// Adapter — Identity module.
/// Translates the integration contract into SendWelcomeEmailCommand and delegates.
/// No business logic here — contract changes affect only this file.
/// </summary>
internal sealed class SendWelcomeEmailOnUserCreatedHandler
    : IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>
{
    private readonly ISendWelcomeEmailCommandHandler _handler;

    public SendWelcomeEmailOnUserCreatedHandler(ISendWelcomeEmailCommandHandler handler)
    {
        _handler = handler;
    }

    public Task HandleAsync(
        UserCreatedFromInvitationIntegrationEvent @event,
        CancellationToken                         ct)
        => _handler.ExecuteAsync(
            new SendWelcomeEmailCommand(@event.TenantId, @event.UserId, @event.Email),
            ct);
}
