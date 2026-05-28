using Atlas.BuildingBlocks.Infrastructure.Metrics;
using Atlas.Identity.Application.Aggregates.Tenants;
using Atlas.Identity.Application.Aggregates.Tenants._Roles._Permissions.Handlers.Queries.ListPermissions;
using Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Commands.CreateRole;
using Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Commands.RemoveRole;
using Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Commands.UpdateRole;
using Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Queries.GetRoleById;
using Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Queries.ListRoles;
using Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Queries.LookupRoles;
using Atlas.Identity.Application.Aggregates.Tenants.Handlers.Commands.ResolveTenantAccess;
using Atlas.Identity.Application.Aggregates.Tenants.MetricMappers;
using Atlas.Identity.Infrastructure.Aggregates.Tenants._Roles._Permissions.Readers.ListPermissions;
using Atlas.Identity.Infrastructure.Aggregates.Tenants._Roles.Readers.GetRoleById;
using Atlas.Identity.Infrastructure.Aggregates.Tenants._Roles.Readers.ListRoles;
using Atlas.Identity.Infrastructure.Aggregates.Tenants._Roles.Readers.LookupRoles;
using Atlas.SharedKernel.Application.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.Aggregates.Tenants;

internal static class TenantServicesExtensions
{
    internal static IServiceCollection AddTenantAggregateServices(this IServiceCollection services)
    {
        // Repository
        services.AddScoped<ITenantRepository, TenantRepository>();

        // Readers
        services.AddScoped<IListRolesReader,       ListRolesReader>();
        services.AddScoped<IGetRoleByIdReader,      GetRoleByIdReader>();
        services.AddScoped<ILookupRolesReader,      LookupRolesReader>();
        services.AddScoped<IListPermissionsReader,  ListPermissionsReader>();

        // Query Handlers
        services.AddScoped<IListRolesQueryHandler,       ListRolesQueryHandler>();
        services.AddScoped<IGetRoleByIdQueryHandler,     GetRoleByIdQueryHandler>();
        services.AddScoped<ILookupRolesQueryHandler,     LookupRolesQueryHandler>();
        services.AddScoped<IListPermissionsQueryHandler, ListPermissionsQueryHandler>();

        // Command Handlers
        services.AddScoped<IResolveTenantAccessCommandHandler, ResolveTenantAccessCommandHandler>();
        services.AddScoped<ICreateRoleCommandHandler,          CreateRoleCommandHandler>();
        services.AddScoped<IRemoveRoleCommandHandler,          RemoveRoleCommandHandler>();
        services.AddScoped<IUpdateRoleCommandHandler,          UpdateRoleCommandHandler>();

        // Metrics
        services.AddScoped<IDomainEventMetricsPublisher, DomainEventMetricsPublisher>();
        services.AddScoped<IMetricMapper,                UserCreatedMetricMapper>();

        return services;
    }
}
