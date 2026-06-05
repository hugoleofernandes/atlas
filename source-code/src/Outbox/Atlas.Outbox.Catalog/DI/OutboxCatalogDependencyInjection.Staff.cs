using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Catalog.DI.TargetCatalog;

internal static class OutboxCatalogDependencyInjectionStaff
{
    internal static IServiceCollection AddStaffTargetCatalogDependencies(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        //services.AddSingleton<IOutboxTargetCatalog, UserCreatedFromInvitationDirectTargetCatalog>();
        //services.AddSingleton<IOutboxTargetCatalog, UserInvitedDirectTargetCatalog>();

        return services;
    }
}
