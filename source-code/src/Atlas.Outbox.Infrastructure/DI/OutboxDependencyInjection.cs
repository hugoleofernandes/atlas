using System.Reflection;
using Atlas.BuildingBlocks.Application.Invokers;
using Atlas.Outbox.Infrastructure;
using Atlas.Outbox.Infrastructure.Configuration;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Infrastructure.DI;

public static class OutboxDependencyInjection
{
    public static IServiceCollection AddOutboxWorker(
        this IServiceCollection services,
        IConfiguration configuration,
        IEnumerable<Assembly> integrationEventAssemblies)
    {
        services.Configure<OutboxWorkerOptions>(configuration.GetSection("OutboxWorker"));

        // No-op request context — satisfaz MultiTenantDbContext sem HTTP request
        services.AddScoped<IRequestContext, WorkerRequestContext>();

        // Invoker — orquestra logging + telemetria para cada handler chamado
        services.AddScoped<IHandlerInvoker, HandlerInvoker>();

        // Dispatcher de integration events (type resolver + invocação de handlers)
        services.AddOutboxInfrastructure(integrationEventAssemblies);

        return services;
    }
}
