using System.Diagnostics;
using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.BuildingBlocks.Application.Seeding;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.Identity.Infrastructure.DI.Aggregates;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Identity.Infrastructure.Seeders;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Domain.Permissions;
using Atlas.Staff.Domain.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        services.AddSingleton<IPermissionPolicy>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<PermissionPolicyService>>();

            // Start before GetServices — the IModulePermissions singletons are constructed here,
            // which triggers PermissionExtractor reflection (const scanning) on each module type.
            var sw      = Stopwatch.StartNew();
            var modules = sp.GetServices<IModulePermissions>().ToList();
            var policy  = new PermissionPolicyService(modules);
            sw.Stop();

            logger.LogInformation(
                "Permission catalog built in {ElapsedMs} ms — {PermissionCount} codes, {GroupCount} groups, {ModuleCount} modules",
                sw.ElapsedMilliseconds,
                policy.All.Count,
                policy.Groups.Count,
                modules.Count);

            return policy;
        });

        services.AddSingleton<IModulePermissions, StaffPermissions>();//todo: - move to Staff.DI module when it exists

        // AGGREGATES
        services.AddTenantAggregateServices();
        services.AddInvitationAggregateServices();
        services.AddUserAggregateServices();

        return services;
    }
}
