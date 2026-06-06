using Atlas.BuildingBlocks.Persistence.Entities.OutboxMessages.Repositories;
using Atlas.Outbox.Application.Commands.ProcessOutbox;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Dispatching;
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Infrastructure.DI;

public static partial class OutboxDependencyInjection
{
    public static IServiceCollection AddModuleOutboxCommandHandler<TCommandHandler, TDbContext>(
        this IServiceCollection services
    )
        where TCommandHandler : class, IProcessOutboxCommandHandler
        where TDbContext : DbContext
    {
        services.AddScoped<TCommandHandler>(sp =>
            (TCommandHandler)
                (object)
                    new ProcessOutboxCommandHandler(
                        sp.GetRequiredService<OutboxMessageRepository<TDbContext>>(),
                        sp.GetRequiredService<IOutboxMessageDispatcher>(),
                        sp.GetRequiredService<IDispatcherInvoker>(),
                        new OutboxUnitOfWork(sp.GetRequiredService<TDbContext>()),
                        sp.GetRequiredService<IRequestContextSetter>(),
                        sp.GetRequiredService<ITraceContextSetter>()
                    )
        );

        return services;
    }
}
