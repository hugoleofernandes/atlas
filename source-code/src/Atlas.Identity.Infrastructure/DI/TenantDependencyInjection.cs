using Atlas.BuildingBlocks.Application.OutboxMessages;
using Atlas.BuildingBlocks.Infrastructure.Metrics;
using Atlas.Identity.Application.Tenants.Commands.InviteUser;
using Atlas.Identity.Application.Tenants.IntegrationEventMappers;
using Atlas.Identity.Application.Tenants.MetricMappers;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Infrastructure.Entities.Tenants.Repositories;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using CreateRole          = Atlas.Identity.Application.Tenants.Commands.CreateRole;
using InviteUser          = Atlas.Identity.Application.Tenants.Commands.InviteUser;
using ResolveTenantAccess = Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

namespace Atlas.Identity.Infrastructure.DI;

public static class TenantDependencyInjection
{
    public static IServiceCollection AddTenantDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InvitationSettings>(configuration.GetSection("Invitations"));

        // COMMAND HANDLERS
        services.AddScoped<ResolveTenantAccess.IResolveTenantAccessCommandHandler, ResolveTenantAccess.ResolveTenantAccessCommandHandler>();
        services.AddScoped<InviteUser.IInviteUserCommandHandler,                   InviteUser.InviteUserCommandHandler>();
        services.AddScoped<CreateRole.ICreateRoleCommandHandler,                   CreateRole.CreateRoleCommandHandler>();

        // OUTBOX
        services.AddScoped<IIntegrationEventEnqueuer, IntegrationEventEnqueuer>();

        // REPOSITORIES
        services.AddScoped<ITenantRepository, TenantRepository>();

        // INTEGRATION EVENT MAPPERS
        services.AddScoped<IIntegrationEventMapper, UserCreatedFromInvitationMapper>();

        // METRICS
        services.AddScoped<IDomainEventMetricsPublisher, DomainEventMetricsPublisher>();
        services.AddScoped<IMetricMapper, UserCreatedMetricMapper>();

        return services;
    }
}
