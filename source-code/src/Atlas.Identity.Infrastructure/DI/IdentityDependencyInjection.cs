using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.BuildingBlocks.Application.Seeding;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Infrastructure.DI.Aggregates;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Identity.Infrastructure.Seeders;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI;

public static class IdentityDependencyInjection
{
    public static IServiceCollection AddIdentityModuleDependencies(this IServiceCollection services)
    {
        // GENERAL
        services.AddScoped<IModuleSeeder, IdentityModuleSeeder>();
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
        services.AddScoped<IHandlerInvoker, HandlerInvoker>();//todo: move to BuildingBlocks.DI when it exists

        // AGGREGATES
        services.AddTenantAggregateServices();
        services.AddInvitationAggregateServices();
        services.AddUserAggregateServices();

        return services;
    }
}
