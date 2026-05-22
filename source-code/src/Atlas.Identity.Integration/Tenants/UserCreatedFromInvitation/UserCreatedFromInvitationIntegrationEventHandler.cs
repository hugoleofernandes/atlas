using Atlas.Identity.Application.Tenants.Commands.SendWelcomeEmail;
using Atlas.Integration.Contracts.Tenants;
using Atlas.SharedKernel.Application.IntegrationEvents;

namespace Atlas.Identity.Integration.Tenants.UserCreatedFromInvitation;

/// <summary>
/// Consume-side adapter for <see cref="UserCreatedFromInvitationIntegrationEvent"/>.
///
/// Responsibilities (adapter only — no business logic here):
///   1. Receive the integration contract from the OutboxWorker dispatcher.
///   2. Translate it into the application command <see cref="SendWelcomeEmailCommand"/>.
///   3. Delegate to <see cref="ISendWelcomeEmailCommandHandler"/> (pure application logic).
///
/// Lives in Atlas.Identity.Integration (not Application) because it knows the shape
/// of the integration contract. If the contract adds or renames a field, only this
/// file changes — SendWelcomeEmailCommand and its handler stay untouched.
/// </summary>
internal sealed class UserCreatedFromInvitationIntegrationEventHandler
    : IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>
{
    private readonly ISendWelcomeEmailCommandHandler _handler;

    public UserCreatedFromInvitationIntegrationEventHandler(
        ISendWelcomeEmailCommandHandler handler)
    {
        _handler = handler;
    }

    public async Task HandleAsync(
        UserCreatedFromInvitationIntegrationEvent @event,
        CancellationToken                         ct)
    {
        var command = new SendWelcomeEmailCommand(
            TenantId: @event.TenantId,
            UserId:   @event.UserId,
            Email:    @event.Email);

        await _handler.ExecuteAsync(command, ct);
    }
}
