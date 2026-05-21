using Atlas.Contracts.Tenants.IntegrationEvents;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Application.Tenants.Services.IntegrationEventHandlers;

/// <summary>
/// Handles the integration event raised when a user completes registration via an invitation link.
/// Triggered by the OutboxWorker after it reads the corresponding OutboxMessage from the database.
/// Responsibility of the Identity module: send welcome email (or other identity-specific reactions).
/// </summary>
public sealed class UserCreatedFromInvitationIntegrationEventHandler
    : IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>
{
    private readonly ILogger<UserCreatedFromInvitationIntegrationEventHandler> _logger;

    public UserCreatedFromInvitationIntegrationEventHandler(
        ILogger<UserCreatedFromInvitationIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(UserCreatedFromInvitationIntegrationEvent @event, CancellationToken ct)
    {
        // 👇 coloca o breakpoint aqui para ver o evento a chegar
        _logger.LogInformation(
            "User created from invitation — TenantId={TenantId} UserId={UserId} Email={Email} Role={Role}",
            @event.TenantId,
            @event.UserId,
            @event.Email,
            @event.Role);

        // TODO: send welcome email via IEmailService

        return Task.CompletedTask;
    }
}
