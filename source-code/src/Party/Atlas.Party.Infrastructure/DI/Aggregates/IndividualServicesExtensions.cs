using Atlas.Party.Application.Commands.DeactivateIndividual;
using Atlas.Party.Application.Commands.RegisterIndividual;
using Atlas.Party.Application.Commands.UpdateIndividual;
using Atlas.Party.Application.Queries.Individuals.GetIndividualById;
using Atlas.Party.Application.Queries.Individuals.ListIndividuals;
using Atlas.Party.Application.Repositories;
using Atlas.Party.Infrastructure.Readers.Individuals.GetIndividualById;
using Atlas.Party.Infrastructure.Readers.Individuals.ListIndividuals;
using Atlas.Party.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Party.Infrastructure.DI.Aggregates;

internal static class IndividualServicesExtensions
{
    internal static IServiceCollection AddIndividualAggregateServices(this IServiceCollection services)
    {
        services.AddScoped<IIndividualRepository, IndividualRepository>();

        services.AddScoped<IGetIndividualByIdReader, GetIndividualByIdReader>();
        services.AddScoped<IListIndividualsReader, ListIndividualsReader>();

        services.AddScoped<IGetIndividualByIdQueryHandler, GetIndividualByIdQueryHandler>();
        services.AddScoped<IListIndividualsQueryHandler, ListIndividualsQueryHandler>();

        services.AddScoped<IRegisterIndividualCommandHandler, RegisterIndividualCommandHandler>();
        services.AddScoped<IUpdateIndividualCommandHandler, UpdateIndividualCommandHandler>();
        services.AddScoped<IDeactivateIndividualCommandHandler, DeactivateIndividualCommandHandler>();

        return services;
    }
}
