using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Queries.GetRoleById;
using Atlas.Identity.Application.Tenants.Queries.ListRoles;
using Atlas.Identity.Application.Tenants.Workflows.RemoveRole;
using Atlas.Identity.Application.Tenants.Workflows.UpdateRole;
using Atlas.Identity.Infrastructure.Entities.Tenants.Queries.GetRoleById;
using Atlas.Identity.Infrastructure.Entities.Tenants.Queries.ListRoles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using RemoveRoleCommandHandler = Atlas.Identity.Application.Tenants.Commands.RemoveRole.RemoveRoleCommandHandler;
using UpdateRoleCommandHandler = Atlas.Identity.Application.Tenants.Commands.UpdateRole.UpdateRoleCommandHandler;

namespace Atlas.Identity.Infrastructure.DI;

public static class IdentityDependencyInjection
{
    public static IServiceCollection AddIdentityModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();

        // Readers (Infrastructure — EF puro)
        services.AddScoped<IListRolesReader, ListRolesReader>();
        services.AddScoped<IGetRoleByIdReader, GetRoleByIdReader>();

        // Query Handlers (Application — orquestração)
        services.AddScoped<IListRolesQueryHandler, ListRolesQueryHandler>();
        services.AddScoped<IGetRoleByIdQueryHandler, GetRoleByIdQueryHandler>();

        // Command Handlers
        services.AddScoped<Atlas.Identity.Application.Tenants.Commands.RemoveRole.IRemoveRoleCommandHandler, RemoveRoleCommandHandler>();
        services.AddScoped<Atlas.Identity.Application.Tenants.Commands.UpdateRole.IUpdateRoleCommandHandler, UpdateRoleCommandHandler>();

        // Workflows
        services.AddScoped<IRemoveRoleWorkflow, RemoveRoleWorkflow>();
        services.AddScoped<IUpdateRoleWorkflow, UpdateRoleWorkflow>();

        return services;
    }
}
