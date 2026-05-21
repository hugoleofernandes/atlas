using Atlas.BuildingBlocks.Infrastructure.Metrics;
using Atlas.Identity.Application.Tenants.Commands.CreateRole;
using Atlas.Identity.Application.Tenants.Commands.InviteUser;
using Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;
using Atlas.Identity.Application.Tenants.MetricMappers;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Infrastructure.Entities.Tenants.Repositories;
using Atlas.SharedKernel.Application.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI;

public static class TenantDependencyInjection
{
    public static IServiceCollection AddTenantDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InvitationSettings>(configuration.GetSection("Invitations"));

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
