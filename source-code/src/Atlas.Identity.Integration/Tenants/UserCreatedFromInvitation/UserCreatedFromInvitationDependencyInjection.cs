using Atlas.Identity.Domain.Entities.Tenants.Events;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Integration.Tenants.UserCreatedFromInvitation;

/// <summary>
/// Publish-side declaration for <see cref="UserCreatedFromInvitationDomainEvent"/>.
///
/// This is the single place that answers: "when this domain event fires,
/// which integration event is published to the outbox?"
///
///   Domain event  — UserCreatedFromInvitationDomainEvent
///   Integration event — UserCreatedFromInvitationIntegrationEvent
///
/// To declare a domain event as an integration event:
///   1. Add the mapper class in this file (or a new per-event file under the matching domain folder).
///   2. Register it via AddScoped below.
///   3. Call the method from IdentityIntegrationDependencyInjection.
///   That's it — the SavePipeline picks it up automatically at runtime.
/// </summary>
internal static class UserCreatedFromInvitationDependencyInjection
{
    internal static IServiceCollection AddUserCreatedFromInvitationMapping(
        this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventMapper, UserCreatedFromInvitationMapper>();

        return services;
    }
}
