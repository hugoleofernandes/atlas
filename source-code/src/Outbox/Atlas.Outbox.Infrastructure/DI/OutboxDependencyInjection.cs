using System.Reflection;
using Atlas.Outbox.Catalog.DI;
using Atlas.Outbox.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Infrastructure.DI;

public static partial class OutboxDependencyInjection
{
    public static IServiceCollection AddOutboxInfrastructureDependencies(
        this IServiceCollection services,
        IConfiguration configuration,
        IEnumerable<Assembly> integrationEventAssemblies
    )
    {
        services.Configure<OutboxWorkerOptions>(configuration.GetSection("OutboxWorker"));

        services.AddOutboxWorkerContexts();
        services.AddOutboxHandlerInfrastructure(configuration, integrationEventAssemblies);

        services.AddOutboxCatalogDependencies(configuration);

        return services;
    }
}
