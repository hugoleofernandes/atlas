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
using Atlas.Outbox.Application.OutboxMessages;
using Atlas.Outbox.Application.ProcessOutbox;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.SharedKernel.Application.Metrics;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.Staff.Application.Abstractions;
using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Infrastructure.Entities.StaffMembers.Repositories;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Infrastructure.DI;

public static class StaffOutboxDependencyInjection
{
    /// <summary>
    /// Registra o suporte ao OutboxWorker para o módulo Staff:
    /// - repositório de outbox (leitura via StaffDbContext)
    /// - ProcessOutboxCommandHandler configurado com deps da Staff
    /// - SavePipeline completo (audit, stampers, metrics) para que handlers
    ///   de integration events passem pelo pipeline correto ao persistir
    /// - dependências de domínio necessárias pelos handlers do Staff
    ///
    /// Os integration event handlers do Staff são registrados em Atlas.Outbox.Integration.
    /// </summary>
    public static IServiceCollection AddStaffOutboxModuleDependencies(this IServiceCollection services)
    {
        // ── Outbox processing ──────────────────────────────────────────────────
        services.AddScoped<OutboxMessageRepository<StaffDbContext>>();

        services.AddScoped<IStaffOutboxCommandHandler>(sp =>
            new ProcessOutboxCommandHandler(
                sp.GetRequiredService<OutboxMessageRepository<StaffDbContext>>(),
                sp.GetRequiredService<IOutboxMessageDispatcher>(),
                new OutboxUnitOfWork(sp.GetRequiredService<StaffDbContext>()),
                sp.GetRequiredService<IRequestContextSetter>()
            ));

        // ── SavePipeline and its dependencies ──────────────────────────────────
        // The full pipeline runs when a Staff integration event handler calls
        // IStaffUnitOfWork.SaveChangesAsync — audit trail, entity stamping,
        // outbox enqueueing and metrics all execute before the DB flush.
        services.AddScoped<IAuditTrailService, AuditTrailService>();
        services.AddScoped<IEntityChangeStamper, EntityChangeStamper>();
        services.AddScoped<IEntityTenantStamper, EntityTenantStamper>();
        services.AddScoped<IOutboxMessageBuilder, OutboxMessageBuilder>();
        services.AddScoped<IDomainEventMetricsPublisher, DomainEventMetricsPublisher>();
        services.AddScoped<ISavePipeline, SavePipeline>();

        // ── Idempotency ────────────────────────────────────────────────────────
        services.AddScoped<IIdempotencyService, IdempotencyService<StaffDbContext>>();

        // ── Staff domain dependencies ──────────────────────────────────────────
        services.AddScoped<IStaffMemberRepository, StaffMemberRepository>();
        services.AddScoped<IStaffUnitOfWork, StaffUnitOfWork>();

        return services;
    }
}
