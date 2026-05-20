using Atlas.BuildingBlocks.Infrastructure.Workflows;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Queries.GetRoleById;
using Atlas.Identity.Application.Tenants.Queries.ListRoles;
using Atlas.Identity.Infrastructure.Entities.Tenants.Queries.GetRoleById;
using Atlas.Identity.Infrastructure.Entities.Tenants.Queries.ListRoles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.Extensions.DependencyInjection;

using RemoveRole = Atlas.Identity.Application.Tenants.Commands.RemoveRole;
using UpdateRole = Atlas.Identity.Application.Tenants.Commands.UpdateRole;

namespace Atlas.Identity.Infrastructure.DI;

public static class IdentityDependencyInjection
{
    public static IServiceCollection AddIdentityModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();

        // INVOKER
        services.AddScoped<IHandlerInvoker, HandlerInvoker>();

        // Readers (Infrastructure — EF puro)
        services.AddScoped<IListRolesReader, ListRolesReader>();
        services.AddScoped<IGetRoleByIdReader, GetRoleByIdReader>();

        // Query Handlers
        services.AddScoped<IListRolesQueryHandler,    ListRolesQueryHandler>();
        services.AddScoped<IGetRoleByIdQueryHandler,  GetRoleByIdQueryHandler>();

        // Command Handlers
        services.AddScoped<RemoveRole.IRemoveRoleCommandHandler, RemoveRole.RemoveRoleCommandHandler>();
        services.AddScoped<UpdateRole.IUpdateRoleCommandHandler, UpdateRole.UpdateRoleCommandHandler>();

        return services;
    }
}
