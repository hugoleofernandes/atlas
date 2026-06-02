using Atlas.BuildingBlocks.Application.ApiInvokers;
using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.Outbox.Infrastructure.Configuration;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Atlas.Outbox.Infrastructure;

public static class OutboxInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IEnumerable<Assembly> integrationEventAssemblies)
    {
        services.AddSingleton<IIntegrationEventTypeResolver>(
            _ => new IntegrationEventTypeResolver(integrationEventAssemblies));

        services.AddHttpClient(nameof(ApiInvoker));
        services.Configure<ApiInvokerOptions>(options =>
        {
            var workerOptions = configuration.GetSection("OutboxWorker").Get<OutboxWorkerOptions>()
                ?? new OutboxWorkerOptions();

            options.InternalApiKey = workerOptions.InternalApiKey;
        });
        services.AddScoped<IApiInvoker, ApiInvoker>();

        // DispatcherInvoker wraps every dispatcher in the generic observability pipeline
        // (LoggingDispatcherDecorator → TracingDispatcherDecorator → core) using ITraceContext.
        // OutboxMessageDispatcher is the core — registered as IOutboxMessageDispatcher so
        // ProcessOutboxCommandHandler can inject it directly and pass it to the invoker.
        services.AddScoped<IOutboxMessageDispatcher>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<OutboxWorkerOptions>>().Value;
            return string.Equals(options.DispatchMode, "Http", StringComparison.OrdinalIgnoreCase)
                ? ActivatorUtilities.CreateInstance<HttpOutboxMessageDispatcher>(sp)
                : ActivatorUtilities.CreateInstance<OutboxMessageDispatcher>(sp);
        });
        services.AddScoped<IDispatcherInvoker, DispatcherInvoker>();

        return services;
    }
}
