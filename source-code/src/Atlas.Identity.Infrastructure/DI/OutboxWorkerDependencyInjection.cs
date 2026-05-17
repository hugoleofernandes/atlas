using Atlas.BuildingBlocks.Persistence.OutboxMessages;
using Atlas.Contracts.Tenants.IntegrationEvents;
using Atlas.Identity.Application.Tenants.Services.IntegrationEventHandlers;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI;

public static class OutboxWorkerDependencyInjection
{
    /// <summary>
    /// Registra o suporte ao OutboxWorker para o módulo Identity:
    /// - repositório de outbox (Identity publica eventos)
    /// - handlers de integration events do módulo
    /// Deve ser chamado no Program.cs do worker junto com AddOutboxWorker().
    /// </summary>
    public static IServiceCollection AddIdentityOutboxWorkerSupport(this IServiceCollection services)
    {
        // Repositório genérico do BuildingBlocks — injeta IdentityDbContext
        services.AddScoped<IOutboxWorkerRepository, OutboxMessageRepository<IdentityDbContext>>();

        // Handlers de integration events — Identity reage a UserCreatedFromInvitation
        services.AddScoped<
            IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>,
            UserCreatedFromInvitationIntegrationEventHandler>();

        return services;
    }
}
