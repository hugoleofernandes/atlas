using Atlas.BuildingBlocks.Application.OutboxMessages;
using Atlas.Identity.Application.Tenants.Commands.InviteUser;
using Atlas.Identity.Application.Tenants.IntegrationEventMappers;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Application.Tenants.Workflows.InviteUser;
using Atlas.Identity.Application.Tenants.Workflows.ResolveTenantAccess;
using Atlas.Identity.Infrastructure.Entities.Tenants.Repositories;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using InviteUser = Atlas.Identity.Application.Tenants.Commands.InviteUser;
using ResolveTenantAccess = Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

namespace Atlas.Identity.Infrastructure.DI;

public static class TenantDependencyInjection
{
    public static IServiceCollection AddTenantDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InvitationSettings>(configuration.GetSection("Invitations"));
        // WORKFLOWS
        services.AddScoped<IResolveTenantAccessWorkflow, ResolveTenantAccessWorkflow>();
        services.AddScoped<IInviteUserWorkflow, InviteUserWorkflow>();

        // COMMAND HANDLERS
        services.AddScoped<ResolveTenantAccess.ICommandHandler, ResolveTenantAccess.CommandHandler>();
        services.AddScoped<InviteUser.ICommandHandler, InviteUser.CommandHandler>();

        // OUTBOX
        services.AddScoped<IIntegrationEventEnqueuer, IntegrationEventEnqueuer>();

        // REPOSITORIES
        services.AddScoped<ITenantRepository, TenantRepository>();

        // INTEGRATION EVENT MAPPERS
        services.AddScoped<IIntegrationEventMapper, UserCreatedFromInvitationMapper>();

        return services;
    }
}
