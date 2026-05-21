using System.Reflection;
using Atlas.BuildingBlocks.Application.Invokers;
using Atlas.BuildingBlocks.Application.Invokers.Interfaces;
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

        // Mutable request context — populated from each OutboxMessage before dispatch.
        // Registered as a concrete type so both IRequestContext and IRequestContextSetter
        // resolve to the same scoped instance.
        services.AddScoped<WorkerRequestContext>();
        services.AddScoped<IRequestContext>(sp        => sp.GetRequiredService<WorkerRequestContext>());
        services.AddScoped<IRequestContextSetter>(sp  => sp.GetRequiredService<WorkerRequestContext>());

        // Invoker — orquestra logging + telemetria para cada handler chamado
        services.AddScoped<IHandlerInvoker, HandlerInvoker>();

        // Dispatcher de integration events (type resolver + invocação de handlers)
        services.AddOutboxInfrastructure(integrationEventAssemblies);

        return services;
    }
}
