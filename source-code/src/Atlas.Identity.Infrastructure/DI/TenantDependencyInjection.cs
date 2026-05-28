using Atlas.BuildingBlocks.Infrastructure.Metrics;
using Atlas.Identity.Application.Invitations.Handlers.Commands.InviteUser;
using Atlas.Identity.Application.Tenants;
using Atlas.Identity.Application.Tenants.Handlers.Commands.ResolveTenantAccess;
using Atlas.Identity.Application.Tenants.MetricMappers;
using Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.CreateRole;
using Atlas.Identity.Infrastructure.Domain.Tenants;
using Atlas.SharedKernel.Application.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI;

public static class TenantDependencyInjection
{
    public static IServiceCollection AddTenantDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // COMMAND HANDLERS
        services.AddScoped<IResolveTenantAccessCommandHandler, ResolveTenantAccessCommandHandler>();
        services.AddScoped<IInviteUserCommandHandler, InviteUserCommandHandler>();
        services.AddScoped<ICreateRoleCommandHandler, CreateRoleCommandHandler>();

        // REPOSITORIES
        services.AddScoped<ITenantRepository, TenantRepository>();

        // METRICS
        services.AddScoped<IDomainEventMetricsPublisher, DomainEventMetricsPublisher>();
        services.AddScoped<IMetricMapper, UserCreatedMetricMapper>();

        return services;
    }
}
