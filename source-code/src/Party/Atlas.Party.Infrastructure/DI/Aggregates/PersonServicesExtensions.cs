using Atlas.Party.Application.Commands.DeactivatePerson;
using Atlas.Party.Application.Commands.RegisterPerson;
using Atlas.Party.Application.Commands.UpdatePerson;
using Atlas.Party.Application.Queries.Persons.GetPersonById;
using Atlas.Party.Application.Queries.Persons.ListPersons;
using Atlas.Party.Application.Repositories;
using Atlas.Party.Infrastructure.Readers.Persons.GetPersonById;
using Atlas.Party.Infrastructure.Readers.Persons.ListPersons;
using Atlas.Party.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Party.Infrastructure.DI.Aggregates;

internal static class PersonServicesExtensions
{
    internal static IServiceCollection AddPersonAggregateServices(this IServiceCollection services)
    {
        services.AddScoped<IPersonRepository, PersonRepository>();

        services.AddScoped<IGetPersonByIdReader, GetPersonByIdReader>();
        services.AddScoped<IListPersonsReader, ListPersonsReader>();

        services.AddScoped<IGetPersonByIdQueryHandler, GetPersonByIdQueryHandler>();
        services.AddScoped<IListPersonsQueryHandler, ListPersonsQueryHandler>();

        services.AddScoped<IRegisterPersonCommandHandler, RegisterPersonCommandHandler>();
        services.AddScoped<IUpdatePersonCommandHandler, UpdatePersonCommandHandler>();
        services.AddScoped<IDeactivatePersonCommandHandler, DeactivatePersonCommandHandler>();

        return services;
    }
}

