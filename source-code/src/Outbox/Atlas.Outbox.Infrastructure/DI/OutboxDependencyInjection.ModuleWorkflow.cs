using Atlas.BuildingBlocks.Outbox.ListPendingMessages;
using Atlas.BuildingBlocks.Persistence.Entities.OutboxMessages.Repositories;
using Atlas.Outbox.Application.Commands.ProcessOutboxTargets;
using Atlas.Outbox.Application.Commands.UpdateOutboxMessageStatus;
using Atlas.Outbox.Application.Queries.ListPendingMessages;
using Atlas.Outbox.Application.Queries.ResolveOutboxTargets;
using Atlas.Outbox.Application.Workflows.OutboxProcessing;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Atlas.Outbox.Infrastructure.DI;

public static partial class OutboxDependencyInjection
{
    public static IServiceCollection AddModuleOutboxProcessingWorkflow<TWorkflow, TDbContext>(
        this IServiceCollection services,
        Func<IServiceProvider, IListPendingMessagesReader> readerFactory
    )
        where TWorkflow : class, IOutboxProcessingWorkflow
        where TDbContext : DbContext
    {
        services.AddScoped<OutboxMessageRepository<TDbContext>>();

        services.AddScoped<TWorkflow>(sp =>
        {
            var repository = sp.GetRequiredService<OutboxMessageRepository<TDbContext>>();
            var unitOfWork = new OutboxUnitOfWork(sp.GetRequiredService<TDbContext>());
            var reader = readerFactory(sp);

            return (TWorkflow)
                (object)
                    new OutboxProcessingWorkflow(
                        new ListPendingMessagesQueryHandler(reader),
                        sp.GetRequiredService<IResolveOutboxTargetsQueryHandler>(),
                        sp.GetRequiredService<IProcessOutboxTargetsCommandHandler>(),
                        new UpdateOutboxMessageStatusCommandHandler(repository, unitOfWork),
                        sp.GetRequiredService<IHandlerInvoker>(),
                        sp.GetRequiredService<ILogger<OutboxProcessingWorkflow>>()
                    );
        });

        return services;
    }
}
