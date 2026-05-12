using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.OutboxMessages.Mappings;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;
using Atlas.Identity.Application.Tenants.UseCases.TenantIntegrationEvents;
using Atlas.Identity.Application.Tenants.Workflows.ResolveTenantAccess;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Identity.Infrastructure.Persistence.Repositories;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();

        services.AddScoped<ITenantIntegrationEventsDispatcher, TenantIntegrationEventsDispatcher>();
        services.AddScoped<ITenantOutboxMappings, TenantOutboxMappings>();


        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        
        services.AddScoped<IResolveTenantAccessWorkflow, ResolveTenantAccessWorkflow>();
        services.AddScoped<IResolveTenantAccessUseCase, ResolveTenantAccessUseCase>();

        return services;
    }
}