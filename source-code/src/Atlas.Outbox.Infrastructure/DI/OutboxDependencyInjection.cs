using System.Reflection;
using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;
using Atlas.Outbox.Infrastructure;
using Atlas.Outbox.Infrastructure.Configuration;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Idempotency;
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

        // Idempotency context — populated per handler invocation by OutboxMessageDispatcher
        // before each pipeline execution.
        // IIdempotencyService is registered per module (needs the module's DbContext).
        services.AddScoped<IdempotencyContext>();
        services.AddScoped<IIdempotencyContext>(sp       => sp.GetRequiredService<IdempotencyContext>());
        services.AddScoped<IIdempotencyContextSetter>(sp => sp.GetRequiredService<IdempotencyContext>());

        // Invoker — routes all handler types (command / query / integration event)
        // through the correct decorator pipeline.
        services.AddScoped<IHandlerInvoker, HandlerInvoker>();

        // Dispatcher — resolves handlers per event type, delegates to invoker
        services.AddOutboxInfrastructure(integrationEventAssemblies);

        return services;
    }
}
