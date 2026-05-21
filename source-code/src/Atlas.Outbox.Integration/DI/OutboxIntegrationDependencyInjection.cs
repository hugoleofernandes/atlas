using Atlas.Outbox.Integration.Tenants;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Integration.DI;

/// <summary>
/// Central registry of all integration event → handler bindings for the Outbox Worker.
///
/// This is the single place that answers: "what does the Outbox Worker do when each
/// integration event arrives?" Each call below corresponds to one event — open the
/// file in the matching domain folder to see which handlers run for that event.
///
///   Tenants/
///     UserCreatedFromInvitation  → Identity: UserCreatedFromInvitationIntegrationEventHandler
///                                  Staff:    CreateStaffMemberIntegrationEventHandler
///
/// To add a new integration event handler:
///   1. Implement IIntegrationEventHandler&lt;TEvent&gt; in the appropriate Application project.
///   2. Create (or open) the file for that event under the matching domain folder.
///   3. Register the handler there and call the method here.
///   4. That's it — the Outbox Worker will pick it up automatically.
/// </summary>
public static class OutboxIntegrationDependencyInjection
{
    public static IServiceCollection AddOutboxIntegrationHandlers(
        this IServiceCollection services)
    {
        services.AddUserCreatedFromInvitationHandlers();

        return services;
    }
}
