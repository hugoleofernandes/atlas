using Atlas.Identity.Application.Tenants.Commands.SendWelcomeEmail;
using Atlas.Identity.Domain.Entities.Tenants.Events;
using Atlas.Integration.Contracts.Tenants;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Integration.Tenants.UserCreatedFromInvitation;

/// <summary>
/// Registration for both sides of the UserCreatedFromInvitation flow.
///
/// Publish side — answers: "when this domain event fires, which integration
///   event is written to the outbox?"
///   DomainEvent  → UserCreatedFromInvitationDomainEvent
///   Outbox entry → UserCreatedFromInvitationIntegrationEvent
///
/// Consume side — answers: "when the OutboxWorker dispatches this integration
///   event, which application command is executed?"
///   Outbox event → UserCreatedFromInvitationIntegrationEvent
///   Command      → SendWelcomeEmailCommand
/// </summary>
internal static class UserCreatedFromInvitationDependencyInjection
{
    internal static IServiceCollection AddUserCreatedFromInvitationMapping(
        this IServiceCollection services)
    {
        // Publish side — SavePipeline picks this up automatically.
        services.AddScoped<IIntegrationEventMapper, UserCreatedFromInvitationMapper>();

        // Consume side — OutboxWorker dispatcher resolves the adapter by
        // IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>.
        services.AddScoped<
            IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>,
            UserCreatedFromInvitationIntegrationEventHandler>();

        // Application command handler called by the adapter above.
        services.AddScoped<ISendWelcomeEmailCommandHandler, SendWelcomeEmailCommandHandler>();

        return services;
    }
}
