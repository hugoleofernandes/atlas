using Atlas.BuildingBlocks.Persistence.Entities.OutboxMessages.Repositories;
using Atlas.BuildingBlocks.Persistence.Entities.Idempotency;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Outbox.Application.Commands.ProcessOutboxTargets;
using Atlas.Outbox.Application.Commands.UpdateOutboxMessageStatus;
using Atlas.Outbox.Application.ProcessOutbox;
using Atlas.Outbox.Application.Queries.GetPendingMessages;
using Atlas.Outbox.Application.Queries.ResolveOutboxTargets;
using Atlas.Outbox.Application.Workflow;
using Atlas.Outbox.Contracts;
using Atlas.Outbox.Infrastructure.Configuration;
using Atlas.Outbox.Infrastructure.Readers.GetPendingMessages;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Dispatching;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Outbox.Infrastructure.DI;

public static class IdentityOutboxDependencyInjection
{
    public static IServiceCollection AddIdentityOutboxModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<OutboxMessageRepository<IdentityDbContext>>();

        services.AddScoped<IIdentityOutboxProcessingWorkflow>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<OutboxWorkerOptions>>().Value;
            var repository = sp.GetRequiredService<OutboxMessageRepository<IdentityDbContext>>();
            var unitOfWork = new OutboxUnitOfWork(sp.GetRequiredService<IdentityDbContext>());

            var reader = string.Equals(options.PendingMessagesMode, "Http", StringComparison.OrdinalIgnoreCase)
                ? (IGetPendingMessagesReader) new HttpGetPendingMessagesReader(
                    sp.GetRequiredService<Atlas.BuildingBlocks.Application.InternalApiInvokers.IInternalApiInvoker>(),
                    sp.GetRequiredService<IOptions<OutboxWorkerOptions>>(),
                    moduleKey: "identity")
                : new IdentityGetPendingMessagesReader(
                    sp.GetRequiredService<IdentityDbContext>());

            return new OutboxProcessingWorkflow(
                new GetPendingMessagesQueryHandler(reader),
                sp.GetRequiredService<IResolveOutboxTargetsQueryHandler>(),
                sp.GetRequiredService<IProcessOutboxTargetsCommandHandler>(),
                new UpdateOutboxMessageStatusCommandHandler(repository, unitOfWork),
                sp.GetRequiredService<IHandlerInvoker>(),
                sp.GetRequiredService<ILogger<OutboxProcessingWorkflow>>());
        });

        services.AddScoped<IIdentityOutboxCommandHandler>(sp =>
            new ProcessOutboxCommandHandler(
                sp.GetRequiredService<OutboxMessageRepository<IdentityDbContext>>(),
                sp.GetRequiredService<IOutboxMessageDispatcher>(),
                sp.GetRequiredService<IDispatcherInvoker>(),
                new OutboxUnitOfWork(sp.GetRequiredService<IdentityDbContext>()),
                sp.GetRequiredService<IRequestContextSetter>(),
                sp.GetRequiredService<ITraceContextSetter>()
            ));

        services.AddScoped<IIdempotencyService, IdempotencyService<IdentityDbContext>>();

        return services;
    }
}
