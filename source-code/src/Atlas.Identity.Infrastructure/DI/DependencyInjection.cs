using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Infrastructure.Persistence;
using Atlas.Identity.Infrastructure.Persistence.IdentityUserConfig;
using Atlas.Identity.Infrastructure.Persistence.TenantConfig;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<IIdentityUserRepository, IdentityUserRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUnitOfWork, IdentityUnitOfWork>();

        return services;
    }
}