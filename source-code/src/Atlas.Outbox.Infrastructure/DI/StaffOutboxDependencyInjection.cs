using Atlas.BuildingBlocks.Application.OutboxMessages;
using Atlas.BuildingBlocks.Infrastructure.Metrics;
using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.BuildingBlocks.Persistence.Entities.Audits.Interfaces;
using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges;
using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Interfaces;
using Atlas.BuildingBlocks.Persistence.Entities.Idempotency;
using Atlas.BuildingBlocks.Persistence.Entities.OutboxMessages.Repositories;
using Atlas.BuildingBlocks.Persistence.Entities.Tenants;
using Atlas.BuildingBlocks.Persistence.Entities.Tenants.Interfaces;
using Atlas.BuildingBlocks.Persistence.Pipelines.Saves;
using Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Interfaces;
using Atlas.Outbox.Application.Commands.ProcessOutboxTargets;
using Atlas.Outbox.Application.Commands.UpdateOutboxMessageStatus;
using Atlas.Outbox.Application.ProcessOutbox;
using Atlas.Outbox.Application.Queries.GetPendingMessages;
using Atlas.Outbox.Application.Queries.ResolveOutboxTargets;
using Atlas.Outbox.Application.Workflow;
using Atlas.Outbox.Contracts;
using Atlas.Outbox.Infrastructure.Readers.GetPendingMessages;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Dispatching;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.SharedKernel.Application.Metrics;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.Staff.Application.Abstractions;
using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Infrastructure.Entities.StaffMembers.Repositories;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Atlas.Outbox.Infrastructure.DI;

public static class StaffOutboxDependencyInjection
{
    /// <summary>
    /// Registra o suporte ao OutboxWorker para o mÃ³dulo Staff:
    /// - repositÃ³rio de outbox (leitura via StaffDbContext)
    /// - ProcessOutboxCommandHandler configurado com deps da Staff
    /// - SavePipeline completo (audit, stampers, metrics) para que handlers
    ///   de integration events passem pelo pipeline correto ao persistir
    /// - dependÃªncias de domÃ­nio necessÃ¡rias pelos handlers do Staff
    ///
    /// Os integration event handlers do Staff sÃ£o registrados em Atlas.Outbox.Integration.
    /// </summary>
    public static IServiceCollection AddStaffOutboxModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<OutboxMessageRepository<StaffDbContext>>();

        services.AddScoped<IStaffOutboxProcessingWorkflow>(sp =>
        {
            var repository = sp.GetRequiredService<OutboxMessageRepository<StaffDbContext>>();
            var unitOfWork = new OutboxUnitOfWork(sp.GetRequiredService<StaffDbContext>());

            // Staff pending-message HTTP endpoint will be added in the next cut.
            // For now Staff keeps reading its outbox directly from the module database.
            IGetPendingMessagesReader reader = new StaffGetPendingMessagesReader(
                sp.GetRequiredService<StaffDbContext>()
            );

            return new OutboxProcessingWorkflow(
                new GetPendingMessagesQueryHandler(reader),
                sp.GetRequiredService<IResolveOutboxTargetsQueryHandler>(),
                sp.GetRequiredService<IProcessOutboxTargetsCommandHandler>(),
                new UpdateOutboxMessageStatusCommandHandler(repository, unitOfWork),
                sp.GetRequiredService<IHandlerInvoker>(),
                sp.GetRequiredService<ILogger<OutboxProcessingWorkflow>>()
            );
        });

        services.AddScoped<IStaffOutboxCommandHandler>(sp => new ProcessOutboxCommandHandler(
            sp.GetRequiredService<OutboxMessageRepository<StaffDbContext>>(),
            sp.GetRequiredService<IOutboxMessageDispatcher>(),
            sp.GetRequiredService<IDispatcherInvoker>(),
            new OutboxUnitOfWork(sp.GetRequiredService<StaffDbContext>()),
            sp.GetRequiredService<IRequestContextSetter>(),
            sp.GetRequiredService<ITraceContextSetter>()
        ));

        // â”€â”€ SavePipeline and its dependencies â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // The full pipeline runs when a Staff integration event handler calls
        // IStaffUnitOfWork.SaveChangesAsync â€” audit trail, entity stamping,
        // outbox enqueueing and metrics all execute before the DB flush.
        services.AddScoped<IAuditTrailService, AuditTrailService>();
        services.AddScoped<IEntityChangeStamper, EntityChangeStamper>();
        services.AddScoped<IEntityTenantStamper, EntityTenantStamper>();
        services.AddScoped<IOutboxMessageBuilder, OutboxMessageBuilder>();
        services.AddScoped<IDomainEventMetricsPublisher, DomainEventMetricsPublisher>();
        services.AddScoped<ISavePipeline, SavePipeline>();

        // â”€â”€ Idempotency â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        services.AddScoped<IIdempotencyService, IdempotencyService<StaffDbContext>>();

        // â”€â”€ Staff domain dependencies â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        services.AddScoped<IStaffMemberRepository, StaffMemberRepository>();
        services.AddScoped<IStaffUnitOfWork, StaffUnitOfWork>();

        return services;
    }
}
