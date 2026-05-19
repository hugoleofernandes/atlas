using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Commands.RemoveRole;
using Atlas.Identity.Application.Tenants.Queries.ListRoles;
using Atlas.Identity.Application.Tenants.Workflows.RemoveRole;
using Atlas.Identity.Infrastructure.Entities.Tenants.Queries.ListRoles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using RemoveRoleCommandHandler = Atlas.Identity.Application.Tenants.Commands.RemoveRole.CommandHandler;

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

        // Command Handlers
        services.AddScoped<ICommandHandler, RemoveRoleCommandHandler>();

        // Workflows
        services.AddScoped<IRemoveRoleWorkflow, RemoveRoleWorkflow>();

        return services;
    }
}