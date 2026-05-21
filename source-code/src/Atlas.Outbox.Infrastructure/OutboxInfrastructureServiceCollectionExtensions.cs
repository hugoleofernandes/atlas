

using Atlas.Outbox.Application.OutboxMessages;
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

        services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher>();

        return services;
    }
}
