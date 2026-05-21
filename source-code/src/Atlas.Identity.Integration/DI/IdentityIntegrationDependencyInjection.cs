using Atlas.Identity.Integration.Tenants.UserCreatedFromInvitation;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Integration.DI;

/// <summary>
/// Central registry of all domain event → integration event mappings for the Identity module.
///
/// This is the single place that answers: "which domain events are published as
/// integration events?" Each call below corresponds to one domain event — open the
/// file in the matching domain folder to see the mapper and the integration event it produces.
///
///   Tenants/
///     UserCreatedFromInvitation  → UserCreatedFromInvitationIntegrationEvent
///
/// To declare a new domain event as an integration event:
///   1. Create (or open) the file for that event under the matching domain folder.
///   2. Add the mapper class and DI registration there.
///   3. Call the method here.
///   That's it — the SavePipeline picks it up automatically at runtime.
/// </summary>
public static class IdentityIntegrationDependencyInjection
{
    public static IServiceCollection AddIdentityIntegrationMappings(
        this IServiceCollection services)
    {
        services.AddUserCreatedFromInvitationMapping();

        return services;
    }
}
