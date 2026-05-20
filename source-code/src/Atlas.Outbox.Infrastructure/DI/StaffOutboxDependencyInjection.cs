using Atlas.BuildingBlocks.Persistence.OutboxMessages;
using Atlas.Contracts.Tenants.IntegrationEvents;
using Atlas.Outbox.Application.OutboxMessages;
using Atlas.Outbox.Application.ProcessOutbox;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.Staff.Application.IntegrationEventHandlers;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Infrastructure.DI;

public static class StaffOutboxDependencyInjection
{
    /// <summary>
    /// Registra o suporte ao OutboxWorker para o módulo Staff:
    /// - repositório de outbox (leitura + save via StaffDbContext)
    /// - ProcessOutboxCommandHandler configurado com deps da Staff
    /// - handlers de integration events consumidos por este módulo
    /// </summary>
    public static IServiceCollection AddStaffOutboxModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<OutboxMessageRepository<StaffDbContext>>();

        services.AddScoped<IStaffOutboxCommandHandler>(sp =>
            new ProcessOutboxCommandHandler(
                sp.GetRequiredService<OutboxMessageRepository<StaffDbContext>>(),
                sp.GetRequiredService<IOutboxMessageDispatcher>()
            ));

        // Integration Event Handlers
        services.AddScoped<IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>,CreateStaffMemberIntegrationEventHandler>();

        return services;
    }
}
