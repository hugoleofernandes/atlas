using Atlas.Identity.Application.Tenants.Abstractions;
using Atlas.Identity.Application.Users.Abstractions;
using Atlas.Identity.Infrastructure.Persistence;
using Atlas.Identity.Infrastructure.Persistence.Tenants;
using Atlas.Identity.Infrastructure.Persistence.Users;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUnitOfWork, IdentityUnitOfWork>();

        return services;
    }
}