using System.Reflection;
using Atlas.OutboxWorker.Configuration;
using Atlas.OutboxWorker.Dispatching;
using Atlas.OutboxWorker.Hosting;
using Atlas.OutboxWorker.Infrastructure;
using Atlas.OutboxWorker.Processing;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.OutboxWorker.DI;

public static class OutboxWorkerServiceCollectionExtensions
{
    /// <summary>
    /// Registra o core do OutboxWorker.
    /// Os repositórios de cada módulo devem ser registrados separadamente
    /// via AddXxxOutboxWorkerSupport() de cada módulo.
    /// </summary>
    public static IServiceCollection AddOutboxWorker(
        this IServiceCollection services,
        IConfiguration configuration,
        IEnumerable<Assembly> integrationEventAssemblies)
    {
        services.Configure<OutboxWorkerOptions>(configuration.GetSection("OutboxWorker"));

        // No-op request context — satisfaz MultiTenantDbContext sem HTTP request
        services.AddScoped<IRequestContext, WorkerRequestContext>();

        // Core worker services
        services.AddScoped<IOutboxProcessor, OutboxProcessor>();
        services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher>();
        services.AddSingleton<IIntegrationEventTypeResolver>(
            _ => new IntegrationEventTypeResolver(integrationEventAssemblies));

        services.AddHostedService<OutboxWorkerHostedService>();

        return services;
    }
}
