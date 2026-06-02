using Atlas.BuildingBlocks.Infrastructure.Metrics;
using Atlas.Identity.Application.Commands.ActivateRole;
using Atlas.Identity.Application.Commands.CreateRole;
using Atlas.Identity.Application.Commands.DeactivateRole;
using Atlas.Identity.Application.Commands.DevLogin;
using Atlas.Identity.Application.Commands.RemoveRole;
using Atlas.Identity.Application.Commands.ResolveTenantAccess;
using Atlas.Identity.Application.Commands.SendWelcomeEmail;
using Atlas.Identity.Application.Commands.UpdateRole;
using Atlas.Identity.Application.MetricMappers;
using Atlas.Identity.Application.Queries.Audit.ListEntries;
using Atlas.Identity.Application.Queries.Permissions.ListPermissions;
using Atlas.Identity.Application.Queries.Roles.GetRoleById;
using Atlas.Identity.Application.Queries.Roles.ListRoles;
using Atlas.Identity.Application.Queries.Roles.LookupRoles;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Infrastructure.Readers.Audit.ListEntries;
using Atlas.Identity.Infrastructure.Readers.Permissions.ListPermissions;
using Atlas.Identity.Infrastructure.Readers.Roles.GetRoleById;
using Atlas.Identity.Infrastructure.Readers.Roles.ListRoles;
using Atlas.Identity.Infrastructure.Readers.Roles.LookupRoles;
using Atlas.Identity.Infrastructure.Repositories;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI.Aggregates;

internal static class TenantServicesExtensions
{
    internal static IServiceCollection AddTenantAggregateServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IRoleRepository, RoleRepository>();

        // Readers
        services.AddScoped<IListRolesReader, ListRolesReader>();
        services.AddScoped<IGetRoleByIdReader, GetRoleByIdReader>();
        services.AddScoped<ILookupRolesReader, LookupRolesReader>();
        services.AddScoped<IListPermissionsReader, ListPermissionsReader>();

        // Audit reader registered as concrete type to avoid DI conflict with
        // other modules that also register IListAuditEntriesReader.
        services.AddScoped<IdentityAuditEntriesReader>();

        // Query Handlers
        services.AddScoped<IListRolesQueryHandler, ListRolesQueryHandler>();
        services.AddScoped<IGetRoleByIdQueryHandler, GetRoleByIdQueryHandler>();
        services.AddScoped<ILookupRolesQueryHandler, LookupRolesQueryHandler>();
        services.AddScoped<IListPermissionsQueryHandler, ListPermissionsQueryHandler>();

        // Factory lambda wires the Identity-specific reader into the shared audit handler
        // without exposing IListAuditEntriesReader in the root DI container.
        services.AddScoped<IIdentityListAuditEntriesQueryHandler>(sp => new IdentityListAuditEntriesQueryHandler(
            sp.GetRequiredService<IdentityAuditEntriesReader>(),
            sp.GetRequiredService<IRequestContext>()
        ));

        // Command Handlers
        services.AddScoped<IDevLoginCommandHandler, DevLoginCommandHandler>();
        services.AddScoped<IResolveTenantAccessCommandHandler, ResolveTenantAccessCommandHandler>();
        services.AddScoped<IActivateRoleCommandHandler, ActivateRoleCommandHandler>();
        services.AddScoped<ICreateRoleCommandHandler, CreateRoleCommandHandler>();
        services.AddScoped<IDeactivateRoleCommandHandler, DeactivateRoleCommandHandler>();
        services.AddScoped<IRemoveRoleCommandHandler, RemoveRoleCommandHandler>();
        services.AddScoped<ISendWelcomeEmailCommandHandler, SendWelcomeEmailCommandHandler>();
        services.AddScoped<IUpdateRoleCommandHandler, UpdateRoleCommandHandler>();

        // Metrics
        services.AddScoped<IDomainEventMetricsPublisher, DomainEventMetricsPublisher>();
        services.AddScoped<IMetricMapper, UserCreatedMetricMapper>();

        return services;
    }
}
