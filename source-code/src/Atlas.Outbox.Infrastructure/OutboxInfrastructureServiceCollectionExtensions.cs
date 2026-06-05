using Atlas.BuildingBlocks.Application.ApiInvokers;
using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.BuildingBlocks.Application.InternalApiInvokers;
using Atlas.Outbox.Application.DirectTargets;
using Atlas.Outbox.Application.Commands.ProcessOutboxTargets;
using Atlas.Outbox.Application.Queries.ResolveOutboxTargets;
using Atlas.Outbox.DirectTargets.IdentityEvents.UserCreatedFromInvitation;
using Atlas.Outbox.DirectTargets.IdentityEvents.UserCreatedFromInvitation.Identity;
using Atlas.Outbox.DirectTargets.IdentityEvents.UserCreatedFromInvitation.Staff;
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
        services.AddHttpClient(nameof(InternalApiInvoker));
        services.Configure<ApiInvokerOptions>(options =>
        {
            var workerOptions = configuration.GetSection("OutboxWorker").Get<OutboxWorkerOptions>()
                ?? new OutboxWorkerOptions();

            options.InternalApiKey = workerOptions.InternalApiKey;
        });
        services.Configure<InternalApiInvokerOptions>(options =>
        {
            var workerOptions = configuration.GetSection("OutboxWorker").Get<OutboxWorkerOptions>()
                ?? new OutboxWorkerOptions();

            options.InternalApiKey = workerOptions.InternalApiKey;
        });
        services.AddScoped<IApiInvoker, ApiInvoker>();
        services.AddScoped<IInternalApiInvoker, InternalApiInvoker>();
        services.AddSingleton<IDirectOutboxTargetCatalog, UserCreatedFromInvitationDirectTargetCatalog>();
        services.AddScoped<IOutboxTargetResolver, DirectOutboxTargetResolver>();
        services.AddScoped<IResolveOutboxTargetsQueryHandler, ResolveOutboxTargetsQueryHandler>();
        services.AddScoped<IProcessOutboxTargetsCommandHandler, ProcessOutboxTargetsCommandHandler>();
        services.AddScoped<IOutboxTargetExecutor, CreateStaffMemberFromInvitationDirectTargetExecutor>();
        services.AddScoped<IOutboxTargetExecutor, SendWelcomeEmailDirectTargetExecutor>();

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
