using Atlas.Identity.OutboxPublisher.Invitations.UserInvited;
using Atlas.Identity.OutboxPublisher.Users.UserCreatedFromInvitation;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.OutboxPublisher.DI;

/// <summary>
/// Registers all Identity domain event → outbox message mappers.
/// Call this from Atlas.API (SavePipeline picks up IIntegrationEventMapper automatically).
///
///   Tenants/
///     UserCreatedFromInvitation  → UserCreatedFromInvitationIntegrationEvent
///
/// To publish a new domain event as an integration event:
///   1. Create a folder under the matching aggregate root (e.g. Tenants/).
///   2. Add the mapper there.
///   3. Call its registration method below.
/// </summary>
public static class IdentityOutboxPublisherDependencyInjection
{
    public static IServiceCollection AddIdentityOutboxPublisherMappings(this IServiceCollection services)
    {
        services.AddUserInvitedMapper();
        services.AddUserCreatedFromInvitationMapper();

        return services;
    }
}
