using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI;

public static class IdentityDependencyInjection
{
    public static IServiceCollection AddIdentityModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();

        return services;
    }
}