using System.Reflection;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Identity.Infrastructure.Readers.Outbox.GetPendingMessages;
using Atlas.Outbox.Application.Workflows.OutboxProcessing;
using Atlas.Outbox.Catalog.DI;
using Atlas.Outbox.Infrastructure.Configuration;
using Atlas.SharedKernel.Application.Dispatching;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Atlas.Staff.Infrastructure.Readers.Outbox.ListPendingMessages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Atlas.Outbox.Infrastructure.DI;

/// <summary>
/// Registers the outbox processing workflows so that Atlas.API can trigger on-demand processing
/// via the /process endpoints without running the dedicated Outbox.Service host.
///
/// Does NOT register WorkerRequestContext or IdempotencyContext — Atlas.API already provides
/// IRequestContext, IRequestContextSetter, IIdempotencyContext, and IIdempotencyContextSetter.
/// Only TraceContext is added here since it has no equivalent in Atlas.API.
/// </summary>
public static class OutboxOnDemandDependencyInjection
{
    public static IServiceCollection AddOutboxOnDemandProcessingDependencies(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // OutboxWorkerOptions — required by AddOutboxInfrastructure (IOptions<OutboxWorkerOptions>).
        services.Configure<OutboxWorkerOptions>(configuration.GetSection("OutboxWorker"));

        // TraceContext — not registered in Atlas.API but required by ProcessOutboxTargetsCommandHandler
        // to correlate dispatcher log/trace entries with the originating outbox message.
        services.TryAddScoped<TraceContext>();
        services.TryAddScoped<ITraceContext>(sp => sp.GetRequiredService<TraceContext>());
        services.TryAddScoped<ITraceContextSetter>(sp => sp.GetRequiredService<TraceContext>());

        // Full outbox dispatch infrastructure: target catalog reader, dispatcher, target executor,
        // integration event type resolver, and the ResolveOutboxTargets + ProcessOutboxTargets handlers.
        // Skips AddOutboxWorkerContexts — Atlas.API already provides the conflicting registrations.
        services.AddOutboxInfrastructure(configuration, GetIntegrationEventAssemblies());

        // Target catalogs (event type → handler mappings) and the concrete target handlers
        // (e.g. SendInvitationEmailTargetHandler, SendWelcomeEmailTargetHandler).
        services.AddOutboxCatalogDependencies(configuration);

        // Per-module processing workflows — hardwired to the Database reader.
        services.AddModuleOutboxProcessingWorkflow<IIdentityOutboxProcessingWorkflow, IdentityDbContext>(sp =>
            new IdentityListPendingMessagesReader(sp.GetRequiredService<IdentityDbContext>())
        );

        services.AddModuleOutboxProcessingWorkflow<IStaffOutboxProcessingWorkflow, StaffDbContext>(sp =>
            new StaffListPendingMessagesReader(sp.GetRequiredService<StaffDbContext>())
        );

        return services;
    }

    private static Assembly[] GetIntegrationEventAssemblies()
    {
        // Mirrors what Atlas.Outbox.Service registers — the Contracts assembly that contains
        // all integration event types consumed by the IIntegrationEventTypeResolver.
        var identityContracts = typeof(Atlas.Identity.Contracts.IntegrationEvents.Users.UserInvitedIntegrationEvent).Assembly;
        return [identityContracts];
    }
}
