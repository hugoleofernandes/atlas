using Atlas.BuildingBlocks.Persistence.OutboxMessages;
using Atlas.Contracts.Tenants.IntegrationEvents;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.Staff.Application.IntegrationEventHandlers;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Staff.Infrastructure.DI;

public static class StaffOutboxWorkerDependencyInjection
{
    /// <summary>
    /// Registra o suporte ao OutboxWorker para o módulo Staff:
    /// - handlers de integration events consumidos por este módulo
    /// Deve ser chamado no Program.cs do worker junto com AddOutboxWorker().
    /// Nota: Staff não publica eventos via outbox (ainda), por isso sem IOutboxWorkerRepository.
    /// </summary>
    public static IServiceCollection AddStaffOutboxWorkerSupport(this IServiceCollection services)
    {
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository<StaffDbContext>>();

        services.AddScoped<
            IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>,
            CreateStaffMemberIntegrationEventHandler>();

        return services;
    }
}
