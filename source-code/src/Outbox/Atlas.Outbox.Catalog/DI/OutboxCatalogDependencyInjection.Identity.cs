using Atlas.Outbox.Application.Targets;
using Atlas.Outbox.Catalog.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Catalog.DI;

internal static class OutboxCatalogDependencyInjectionIdentity
{
    internal static IServiceCollection AddIdentityTargetCatalogDependencies(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSingleton<ITargetCatalog, UserCreatedFromInvitationTargetCatalog>();
        services.AddSingleton<ITargetCatalog, UserInvitedTargetCatalog>();

        return services;
    }
}
