using Atlas.BuildingBlocks.Persistence.Entities.OutboxMessages.Repositories;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Outbox.Application.Commands.ResubmitDeadLetter;
using Atlas.Outbox.Application.Queries.ListDeadLetters;
using Atlas.Outbox.Application.Queries.ListOutboxMessages;
using Atlas.Outbox.Infrastructure.Readers.ListDeadLetters;
using Atlas.Outbox.Infrastructure.Readers.ListOutboxMessages;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Atlas.Outbox.Infrastructure.DI;

/// <summary>
/// Registers the outbox management surface — the handlers behind the
/// triage/replay HTTP endpoints (list messages, list dead-letters, resubmit).
/// Called by Atlas.API only; the outbox worker hosts do not expose endpoints
/// and do not need these.
/// </summary>
public static class OutboxManagementDependencyInjection
{
    public static IServiceCollection AddOutboxManagementDependencies(this IServiceCollection services)
    {
        services.TryAddScoped<OutboxMessageRepository<IdentityDbContext>>();
        services.TryAddScoped<OutboxMessageRepository<StaffDbContext>>();

        // IDENTITY
        services.AddModuleResubmitCommandHandler<IIdentityResubmitDeadLetterCommandHandler, IdentityDbContext>();

        services.AddModuleListDeadLettersQueryHandler<IIdentityListDeadLettersQueryHandler>(
            sp => new IdentityListDeadLettersReader(sp.GetRequiredService<IdentityDbContext>())
        );

        services.AddModuleListOutboxMessagesQueryHandler<IIdentityListOutboxMessagesQueryHandler>(
            sp => new IdentityListOutboxMessagesReader(sp.GetRequiredService<IdentityDbContext>())
        );

        // STAFF
        services.AddModuleResubmitCommandHandler<IStaffResubmitDeadLetterCommandHandler, StaffDbContext>();

        services.AddModuleListDeadLettersQueryHandler<IStaffListDeadLettersQueryHandler>(
            sp => new StaffListDeadLettersReader(sp.GetRequiredService<StaffDbContext>())
        );

        services.AddModuleListOutboxMessagesQueryHandler<IStaffListOutboxMessagesQueryHandler>(
            sp => new StaffListOutboxMessagesReader(sp.GetRequiredService<StaffDbContext>())
        );

        return services;
    }
}
