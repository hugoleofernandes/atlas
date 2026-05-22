using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Identity.Application.Tenants.Commands.RemoveRole;
using Atlas.Identity.Application.Tenants.Commands.UpdateRole;
using Atlas.Identity.Application.Tenants.Queries.GetRoleById;
using Atlas.Identity.Application.Tenants.Queries.ListRoles;
using Microsoft.Extensions.DependencyInjection;
using Atlas.Identity.Infrastructure.Entities.Tenants.Readers.GetRoleById;
using Atlas.Identity.Infrastructure.Entities.Tenants.Readers.ListRoles;
using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.SharedKernel.Application.Handlers;

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
        services.AddScoped<IRemoveRoleCommandHandler, RemoveRoleCommandHandler>();
        services.AddScoped<IUpdateRoleCommandHandler, UpdateRoleCommandHandler>();

        return services;
    }
}
