using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Abstractions.Repositories;
using Atlas.Identity.Application.Abstractions.Tenants.Commands.ResolveAccess;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Identity.Infrastructure.Persistence.Repositories;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
        services.AddScoped<IIntegrationEventMapper, IntegrationEventMapper>();

        services.AddScoped<ITenantRepository, TenantRepository>();
        
        services.AddScoped<IResolveAccessWorkflow, ResolveAccessWorkflow>();
        services.AddScoped<IResolveAccessUserCase, ResolveAccessUserCase>();

        return services;
    }
}