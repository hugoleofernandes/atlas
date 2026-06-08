using Atlas.BuildingBlocks.Persistence.Entities.Idempotency;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Identity.Infrastructure.Readers.Outbox.GetPendingMessages;
using Atlas.Outbox.Application.Commands.ProcessOutbox;
using Atlas.Outbox.Application.Workflows.OutboxProcessing;
using Atlas.Outbox.Infrastructure.Configuration;
using Atlas.Outbox.Infrastructure.Readers.ListPendingMessages;
using Atlas.SharedKernel.Application.Idempotency;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Atlas.Outbox.Infrastructure.DI;

public static class IdentityOutboxDependencyInjection
{
    public static IServiceCollection AddIdentityOutboxModuleDependencies(this IServiceCollection services)
    {
        services.AddModuleOutboxProcessingWorkflow<IIdentityOutboxProcessingWorkflow, IdentityDbContext>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<OutboxWorkerOptions>>().Value;

            return string.Equals(options.PendingMessagesMode, "Http", StringComparison.OrdinalIgnoreCase)
                ? new HttpListPendingMessagesReader(
                    sp.GetRequiredService<Atlas.BuildingBlocks.Application.InternalApiInvokers.IInternalApiInvoker>(),
                    sp.GetRequiredService<IOptions<OutboxWorkerOptions>>(),
                    moduleKey: "identity"
                )
                : new IdentityListPendingMessagesReader(sp.GetRequiredService<IdentityDbContext>());
        });

        services.AddModuleOutboxCommandHandler<IIdentityOutboxCommandHandler, IdentityDbContext>();

        services.AddScoped<IIdempotencyService, IdempotencyService<IdentityDbContext>>();

        return services;
    }
}
