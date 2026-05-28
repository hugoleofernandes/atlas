using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.BuildingBlocks.Application.Seeding;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.Identity.Infrastructure.Aggregates.Invitations;
using Atlas.Identity.Infrastructure.Aggregates.Tenants;
using Atlas.Identity.Infrastructure.Aggregates.Users;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Domain.Permissions;
using Atlas.Staff.Domain.Permissions;
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

        // PERMISSION POLICY
        // Each module registers its IModulePermissions; PermissionPolicyService aggregates them.
        services.AddSingleton<IModulePermissions, IdentityModulePermissions>();
        services.AddSingleton<IPermissionPolicy>(sp => new PermissionPolicyService(sp.GetServices<IModulePermissions>()));

        services.AddSingleton<IModulePermissions, StaffPermissions>();//todo: - move to Staff.DI module when it exists

        // AGGREGATES
        services.AddTenantAggregateServices();
        services.AddInvitationAggregateServices();
        services.AddUserAggregateServices();

        return services;
    }
}
