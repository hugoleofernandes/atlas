using Atlas.Outbox.IdentityConsumer.Users.UserCreatedFromInvitation;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.IdentityConsumer._DI;

/// <summary>
/// Registers all handlers for integration events published by the Identity module.
/// Call this from Atlas.Outbox.Worker.
///
/// Convention: the suffix "Identity" refers to the publishing module, not the consuming one.
/// Adapters inside this project may belong to any consuming module (Identity, Staff, etc.)
/// as long as they handle events that originated in the Identity domain.
///
///   Tenants/UserCreatedFromInvitation  → Identity: SendWelcomeEmailOnUserCreatedHandler
///                                        Staff:    CreateStaffMemberOnUserCreatedHandler
///
/// To add a new Identity event: create the matching folder and call its DI method below.
/// </summary>
public static class OutboxIdentityConsumerDependencyInjection
{
    public static IServiceCollection AddOutboxIdentityConsumerHandlers(this IServiceCollection services)
    {
        services.AddUserCreatedFromInvitationConsumeHandlers();

        return services;
    }
}
