using System.Reflection;
using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Infrastructure.DI;

public static partial class OutboxDependencyInjection
{
    private static IServiceCollection AddOutboxHandlerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IEnumerable<Assembly> integrationEventAssemblies
    )
    {
        // Invoker — routes all handler types (command / query / integration event)
        // through the correct decorator pipeline.
        services.AddScoped<IHandlerInvoker, HandlerInvoker>();

        // Dispatcher — resolves handlers per event type, delegates to invoker.
        services.AddOutboxInfrastructure(configuration, integrationEventAssemblies);

        return services;
    }
}
