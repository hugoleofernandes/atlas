using Atlas.Party.Application.Commands.DeactivateOrganization;
using Atlas.Party.Application.Commands.RegisterOrganization;
using Atlas.Party.Application.Commands.UpdateOrganization;
using Atlas.Party.Application.Queries.Organizations.GetOrganizationById;
using Atlas.Party.Application.Queries.Organizations.ListOrganizations;
using Atlas.Party.Application.Repositories;
using Atlas.Party.Infrastructure.Readers.Organizations.GetOrganizationById;
using Atlas.Party.Infrastructure.Readers.Organizations.ListOrganizations;
using Atlas.Party.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Party.Infrastructure.DI.Aggregates;

internal static class OrganizationServicesExtensions
{
    internal static IServiceCollection AddOrganizationAggregateServices(this IServiceCollection services)
    {
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();

        services.AddScoped<IGetOrganizationByIdReader, GetOrganizationByIdReader>();
        services.AddScoped<IListOrganizationsReader, ListOrganizationsReader>();

        services.AddScoped<IGetOrganizationByIdQueryHandler, GetOrganizationByIdQueryHandler>();
        services.AddScoped<IListOrganizationsQueryHandler, ListOrganizationsQueryHandler>();

        services.AddScoped<IRegisterOrganizationCommandHandler, RegisterOrganizationCommandHandler>();
        services.AddScoped<IUpdateOrganizationCommandHandler, UpdateOrganizationCommandHandler>();
        services.AddScoped<IDeactivateOrganizationCommandHandler, DeactivateOrganizationCommandHandler>();

        return services;
    }
}
