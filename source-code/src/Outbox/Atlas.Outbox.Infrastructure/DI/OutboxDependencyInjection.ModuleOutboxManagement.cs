using Atlas.BuildingBlocks.Persistence.Entities.OutboxMessages.Repositories;
using Atlas.Outbox.Application.Commands.ResubmitDeadLetter;
using Atlas.Outbox.Application.Queries.ListDeadLetters;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Infrastructure.DI;

public static partial class OutboxDependencyInjection
{
    public static IServiceCollection AddModuleResubmitCommandHandler<TCommandHandler, TDbContext>(
        this IServiceCollection services
    )
        where TCommandHandler : class, IResubmitDeadLetterCommandHandler
        where TDbContext : DbContext
    {
        services.AddScoped<TCommandHandler>(sp =>
            (TCommandHandler)
                (object)
                    new ResubmitDeadLetterCommandHandler(
                        sp.GetRequiredService<OutboxMessageRepository<TDbContext>>(),
                        sp.GetRequiredService<IRequestContext>(),
                        new OutboxUnitOfWork(sp.GetRequiredService<TDbContext>())
                    )
        );

        return services;
    }

    public static IServiceCollection AddModuleListDeadLettersQueryHandler<TQueryHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, IListDeadLettersReader> readerFactory
    )
        where TQueryHandler : class, IListDeadLettersQueryHandler
    {
        services.AddScoped<TQueryHandler>(sp =>
            (TQueryHandler)
                (object)
                    new ListDeadLettersQueryHandler(readerFactory(sp))
        );

        return services;
    }
}
