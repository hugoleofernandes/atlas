using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Queries.ListRoles;
using Atlas.Identity.Infrastructure.Entities.Tenants.Queries.ListRoles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI;

public static class IdentityDependencyInjection
{
    public static IServiceCollection AddIdentityModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();

        // Readers (Infrastructure — EF puro)
        services.AddScoped<IListRolesReader, ListRolesReader>();

        // Query Handlers (Application — orquestração)
        services.AddScoped<IListRolesQueryHandler, ListRolesQueryHandler>();

        return services;
    }
}