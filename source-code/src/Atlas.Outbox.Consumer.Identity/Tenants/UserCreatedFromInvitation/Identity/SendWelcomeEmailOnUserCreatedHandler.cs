using Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;
using Atlas.Identity.Application.Tenants.Commands.SendWelcomeEmail;
using Atlas.Integration.Contracts.Tenants;
using Atlas.SharedKernel.Application.IntegrationEvents;

namespace Atlas.Outbox.Consumer.Identity.Tenants.UserCreatedFromInvitation.Identity;

/// <summary>
/// Adapter — Identity module.
/// Translates UserCreatedFromInvitationIntegrationEvent → SendWelcomeEmailCommand
/// and delegates through IHandlerInvoker so the command runs the full pipeline
/// (IdempotencyDecorator → ValidationDecorator → PersistDbDecorator → telemetry).
/// No business logic here — contract changes affect only this file.
/// </summary>
internal sealed class SendWelcomeEmailOnUserCreatedHandler
    : IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>
{
    private readonly ISendWelcomeEmailCommandHandler _handler;
    private readonly IHandlerInvoker                _invoker;

    public SendWelcomeEmailOnUserCreatedHandler(
        ISendWelcomeEmailCommandHandler handler,
        IHandlerInvoker                 invoker)
    {
        _handler = handler;
        _invoker  = invoker;
    }

    public async Task HandleAsync(
        UserCreatedFromInvitationIntegrationEvent @event,
        CancellationToken                         ct)
    {
        var result = await _invoker.InvokeAsync(
            _handler,
            new SendWelcomeEmailCommand(@event.TenantId, @event.UserId, @event.Email),
            ct);

        if (!result.IsSuccess)
            throw new InvalidOperationException(
                result.ErrorDefinition?.FallbackMessage ?? "SendWelcomeEmail command failed.");
    }
}
