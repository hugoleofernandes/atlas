using Atlas.BuildingBlocks.Application.OutboxMessages;
using Atlas.BuildingBlocks.Infrastructure.Metrics;
using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.BuildingBlocks.Persistence.Entities.Audits.Interfaces;
using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges;
using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Interfaces;
using Atlas.BuildingBlocks.Persistence.Entities.Idempotency;
using Atlas.BuildingBlocks.Persistence.Entities.Tenants;
using Atlas.BuildingBlocks.Persistence.Entities.Tenants.Interfaces;
using Atlas.BuildingBlocks.Persistence.Pipelines.Saves;
using Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Interfaces;
using Atlas.Outbox.Application.Commands.ProcessOutbox;
using Atlas.Outbox.Application.Commands.ResubmitDeadLetter;
using Atlas.Outbox.Application.Queries.ListDeadLetters;
using Atlas.Outbox.Application.Workflows.OutboxProcessing;
using Atlas.Outbox.Infrastructure.Readers.ListDeadLetters;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.SharedKernel.Application.Metrics;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.Staff.Application.Abstractions;
using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Infrastructure.Entities.StaffMembers.Repositories;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Atlas.Staff.Infrastructure.Readers.Outbox.ListPendingMessages;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Infrastructure.DI;

public static class StaffOutboxDependencyInjection
{
    public static IServiceCollection AddStaffOutboxModuleDependencies(this IServiceCollection services)
    {
        services.AddModuleOutboxProcessingWorkflow<IStaffOutboxProcessingWorkflow, StaffDbContext>(
            sp => new StaffListPendingMessagesReader(sp.GetRequiredService<StaffDbContext>())
        );

        services.AddModuleOutboxCommandHandler<IStaffOutboxCommandHandler, StaffDbContext>();

        services.AddModuleResubmitCommandHandler<IStaffResubmitDeadLetterCommandHandler, StaffDbContext>();

        services.AddModuleListDeadLettersQueryHandler<IStaffListDeadLettersQueryHandler>(
            sp => new StaffListDeadLettersReader(sp.GetRequiredService<StaffDbContext>())
        );

        services.AddScoped<IAuditTrailService, AuditTrailService>();
        services.AddScoped<IEntityChangeStamper, EntityChangeStamper>();
        services.AddScoped<IEntityTenantStamper, EntityTenantStamper>();
        services.AddScoped<IOutboxMessageBuilder, OutboxMessageBuilder>();
        services.AddScoped<IDomainEventMetricsPublisher, DomainEventMetricsPublisher>();
        services.AddScoped<ISavePipeline, SavePipeline>();

        services.AddScoped<IIdempotencyService, IdempotencyService<StaffDbContext>>();

        services.AddScoped<IStaffMemberRepository, StaffMemberRepository>();
        services.AddScoped<IStaffUnitOfWork, StaffUnitOfWork>();

        return services;
    }
}
