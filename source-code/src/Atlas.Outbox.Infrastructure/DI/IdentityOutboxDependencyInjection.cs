using Atlas.BuildingBlocks.Persistence.OutboxMessages;
using Atlas.Contracts.Tenants.IntegrationEvents;
using Atlas.Identity.Application.Tenants.Services.IntegrationEventHandlers;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Outbox.Application.OutboxMessages;
using Atlas.Outbox.Application.ProcessOutbox;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Infrastructure.DI;

public static class IdentityOutboxDependencyInjection
{
    /// <summary>
    /// Registra o suporte ao OutboxWorker para o módulo Identity:
    /// - repositório de outbox (leitura + save via IdentityDbContext)
    /// - ProcessOutboxCommandHandler configurado com deps da Identity
    /// - handlers de integration events consumidos por este módulo
    /// </summary>
    public static IServiceCollection AddIdentityOutboxModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<OutboxMessageRepository<IdentityDbContext>>();

        services.AddScoped<IIdentityOutboxCommandHandler>(sp =>
            new ProcessOutboxCommandHandler(
                sp.GetRequiredService<OutboxMessageRepository<IdentityDbContext>>(),
                sp.GetRequiredService<IOutboxMessageDispatcher>()
            ));

        // Integration Event Handlers
        services.AddScoped<IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>, UserCreatedFromInvitationIntegrationEventHandler>();

        return services;
    }
}
