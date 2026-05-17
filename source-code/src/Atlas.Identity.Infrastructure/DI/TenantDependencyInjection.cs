using Atlas.BuildingBlocks.Application.OutboxMessages;
using Atlas.Identity.Application.Tenants.IntegrationEventMappers;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Application.Tenants.Workflows.ResolveTenantAccess;
using Atlas.Identity.Infrastructure.Entities.Tenants.Repositories;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;

using ResolveTenantAccess = Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

namespace Atlas.Identity.Infrastructure.DI;

public static class TenantDependencyInjection
{
    public static IServiceCollection AddTenantDependencies(this IServiceCollection services)
    {
        // WORKFLOWS
        services.AddScoped<IResolveTenantAccessWorkflow, ResolveTenantAccessWorkflow>();

        // COMMAND HANDLERS
        services.AddScoped<ResolveTenantAccess.ICommandHandler, ResolveTenantAccess.CommandHandler>();

        // OUTBOX
        services.AddScoped<IIntegrationEventEnqueuer, IntegrationEventEnqueuer>();

        // REPOSITORIES
        services.AddScoped<ITenantRepository, TenantRepository>();

        // INTEGRATION EVENT MAPPERS
        services.AddScoped<IIntegrationEventMapper, UserCreatedFromInvitationMapper>();

        return services;
    }
}
