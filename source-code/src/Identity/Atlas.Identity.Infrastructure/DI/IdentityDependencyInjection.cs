using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.BuildingBlocks.Outbox.ListPendingMessages;
using Atlas.BuildingBlocks.Permissions;
using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Infrastructure.Cache;
using Atlas.Identity.Infrastructure.DI.Aggregates;
using Atlas.Identity.Infrastructure.Labels;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Identity.Infrastructure.Readers.Outbox.GetPendingMessages;
using Atlas.Identity.Infrastructure.Readers.Permissions;
using Atlas.Identity.Infrastructure.Seeders;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI;

public static class IdentityDependencyInjection
{
    public static IServiceCollection AddIdentityModuleDependencies(this IServiceCollection services)
    {
        // GENERAL
        services.AddScoped<IdentityModuleSeeder>();
        services.AddScoped<IdentityPermissionCatalogSeeder>();
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
        services.AddScoped<IHandlerInvoker, HandlerInvoker>(); //todo: move to BuildingBlocks.DI when it exists
        services.AddScoped<IAuditLabelProvider, IdentityAuditLabelProvider>();
        services.AddScoped<IPermissionLabelProvider, IdentityPermissionLabelProvider>();
        // Cache singleton — implementação vive na infraestrutura da Identity (não em BuildingBlocks).
        services.AddSingleton<IPermissionCatalogCache, InMemoryPermissionCatalogCache>();
        // Registado apenas pelo tipo concreto — IPermissionCatalogReader não está no DI por design.
        // Ver documentação de IPermissionCatalogReader para o motivo.
        services.AddScoped<PermissionCatalogReader>();

        // OUTBOX — reader + handler for InternalApi pending-messages endpoint
        services.AddScoped<IdentityListPendingMessagesReader>();
        services.AddScoped<IListPendingMessagesQueryHandler>(sp => new ListPendingMessagesQueryHandler(
            sp.GetRequiredService<IdentityListPendingMessagesReader>()
        ));

        // AGGREGATES
        services.AddTenantAggregateServices();
        services.AddInvitationAggregateServices();
        services.AddUserAggregateServices();

        return services;
    }
}
