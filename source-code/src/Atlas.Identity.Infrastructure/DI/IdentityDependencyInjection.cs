using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Staff.Domain.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Domain.Permissions;
using Atlas.Identity.Domain.Tenants.Roles.Permissions;
using Atlas.Identity.Infrastructure.Domain.Tenants;
using Atlas.Identity.Infrastructure.Domain.Tenants.Roles.Readers.GetRoleById;
using Atlas.Identity.Infrastructure.Domain.Tenants.Roles.Readers.ListRoles;
using Atlas.Identity.Infrastructure.Domain.Tenants.Roles.Readers.LookupRoles;
using Atlas.Identity.Infrastructure.Domain.Tenants.Roles.Permissions.Readers.ListPermissions;
using Atlas.Identity.Infrastructure.Domain.Invitations;
using Atlas.Identity.Infrastructure.Domain.Invitations.Readers.ListInvitations;
using Atlas.Identity.Infrastructure.Domain.Users;
using Atlas.Identity.Application.Tenants;
using Atlas.Identity.Application.Invitations;
using Atlas.Identity.Application.Users;
using Atlas.Identity.Application.Tenants.Roles.Handlers.Queries.GetRoleById;
using Atlas.Identity.Application.Tenants.Roles.Handlers.Queries.ListRoles;
using Atlas.Identity.Application.Tenants.Roles.Handlers.Queries.LookupRoles;
using Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.RemoveRole;
using Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.UpdateRole;
using Atlas.Identity.Application.Tenants.Roles.Permissions.Handlers.Queries.ListPermissions;
using Atlas.Identity.Application.Invitations.Handlers.Queries.ListInvitations;
using Atlas.BuildingBlocks.Application.Seeding;
using Atlas.Identity.Infrastructure.Persistence.Seed;

namespace Atlas.Identity.Infrastructure.DI;

public static class IdentityDependencyInjection
{
    public static IServiceCollection AddIdentityModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<IModuleSeeder, IdentityModuleSeeder>();

        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();

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
