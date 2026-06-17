using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.BuildingBlocks.Permissions;
using Atlas.Party.Application.Abstractions;
using Atlas.Party.Infrastructure.DI.Aggregates;
using Atlas.Party.Infrastructure.Labels;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Party.Infrastructure.DI;

public static class PartyDependencyInjection
{
    public static IServiceCollection AddPartyModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<IPartyUnitOfWork, PartyUnitOfWork>();
        services.AddScoped<IHandlerInvoker, HandlerInvoker>(); //todo: move to BuildingBlocks.DI when it exists
        services.AddScoped<IPermissionLabelProvider, PartyPermissionLabelProvider>();

        services.AddPersonAggregateServices();
        services.AddOrganizationAggregateServices();

        return services;
    }
}

