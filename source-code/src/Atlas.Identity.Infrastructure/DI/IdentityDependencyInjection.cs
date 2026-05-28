using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Identity.Application.Tenants.Commands.RemoveRole;
using Atlas.Identity.Application.Tenants.Commands.UpdateRole;
using Atlas.Identity.Application.Tenants.Queries.GetRoleById;
using Atlas.Identity.Application.Tenants.Queries.ListInvitations;
using Atlas.Identity.Application.Tenants.Queries.ListRoles;
using Atlas.Identity.Application.Tenants.Queries.ListPermissions;
using Atlas.Identity.Application.Tenants.Queries.LookupRoles;
using Atlas.Identity.Domain.Entities.Tenants.Roles.Permissions;
using Atlas.Staff.Domain.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Atlas.Identity.Infrastructure.Entities.Tenants.Readers.GetRoleById;
using Atlas.Identity.Infrastructure.Entities.Tenants.Readers.ListInvitations;
using Atlas.Identity.Infrastructure.Entities.Tenants.Readers.ListPermissions;
using Atlas.Identity.Infrastructure.Entities.Tenants.Readers.ListRoles;
using Atlas.Identity.Infrastructure.Entities.Tenants.Readers.LookupRoles;
using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.Identity.Infrastructure.DI;

public static class IdentityDependencyInjection
{
    public static IServiceCollection AddIdentityModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();

        // INVOKER
        services.AddScoped<IHandlerInvoker, HandlerInvoker>();

        // PERMISSION POLICY
        // Each module registers its IModulePermissions; PermissionPolicyService aggregates them.
        services.AddSingleton<IModulePermissions, IdentityPermissions>();
        services.AddSingleton<IModulePermissions, StaffPermissions>();
        services.AddSingleton<IPermissionPolicy>(sp =>
            new PermissionPolicyService(sp.GetServices<IModulePermissions>()));


        // Readers (Infrastructure — EF puro)
        services.AddScoped<IListInvitationsReader, ListInvitationsReader>();
        services.AddScoped<IListRolesReader, ListRolesReader>();
        services.AddScoped<IGetRoleByIdReader, GetRoleByIdReader>();
        services.AddScoped<ILookupRolesReader, LookupRolesReader>();
        services.AddScoped<IListPermissionsReader, ListPermissionsReader>();

        // Query Handlers
        services.AddScoped<IListInvitationsQueryHandler, ListInvitationsQueryHandler>();
        services.AddScoped<IListRolesQueryHandler,       ListRolesQueryHandler>();
        services.AddScoped<IGetRoleByIdQueryHandler,     GetRoleByIdQueryHandler>();
        services.AddScoped<ILookupRolesQueryHandler,     LookupRolesQueryHandler>();
        services.AddScoped<IListPermissionsQueryHandler, ListPermissionsQueryHandler>();

        // Command Handlers
        services.AddScoped<IRemoveRoleCommandHandler, RemoveRoleCommandHandler>();
        services.AddScoped<IUpdateRoleCommandHandler, UpdateRoleCommandHandler>();

        return services;
    }
}
