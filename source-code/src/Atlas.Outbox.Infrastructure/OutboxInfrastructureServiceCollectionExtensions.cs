

using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Atlas.Outbox.Infrastructure;

public static class OutboxInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxInfrastructure(
        this IServiceCollection services,
        IEnumerable<Assembly> integrationEventAssemblies)
    {
        services.AddSingleton<IIntegrationEventTypeResolver>(
            _ => new IntegrationEventTypeResolver(integrationEventAssemblies));

        // DispatcherInvoker wraps every dispatcher in the generic observability pipeline
        // (LoggingDispatcherDecorator → TracingDispatcherDecorator → core) using ITraceContext.
        // OutboxMessageDispatcher is the core — registered as IOutboxMessageDispatcher so
        // ProcessOutboxCommandHandler can inject it directly and pass it to the invoker.
        services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher>();
        services.AddScoped<IDispatcherInvoker, DispatcherInvoker>();

        return services;
    }
}
