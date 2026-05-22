using Atlas.BuildingBlocks.Persistence.Entities.OutboxMessages.Repositories;
using Atlas.BuildingBlocks.Persistence.Entities.Idempotency;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.Outbox.Application.ProcessOutbox;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Dispatching;
using Atlas.SharedKernel.Application.Idempotency;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Infrastructure.DI;

public static class IdentityOutboxDependencyInjection
{
    /// <summary>
    /// Registra o suporte ao OutboxWorker para o módulo Identity:
    /// - repositório de outbox (leitura + save via IdentityDbContext)
    /// - ProcessOutboxCommandHandler configurado com deps da Identity
    /// - idempotência via IdentityDbContext
    ///
    /// Os integration event handlers do Identity são registrados em Atlas.Outbox.Integration.
    /// </summary>
    public static IServiceCollection AddIdentityOutboxModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<OutboxMessageRepository<IdentityDbContext>>();

        services.AddScoped<IIdentityOutboxCommandHandler>(sp =>
            new ProcessOutboxCommandHandler(
                sp.GetRequiredService<OutboxMessageRepository<IdentityDbContext>>(),
                sp.GetRequiredService<IOutboxMessageDispatcher>(),
                sp.GetRequiredService<IDispatcherInvoker>(),
                new OutboxUnitOfWork(sp.GetRequiredService<IdentityDbContext>()),
                sp.GetRequiredService<IRequestContextSetter>(),
                sp.GetRequiredService<ITraceContextSetter>()
            ));

        // ── Idempotency ────────────────────────────────────────────────────────
        services.AddScoped<IIdempotencyService, IdempotencyService<IdentityDbContext>>();

        return services;
    }
}
