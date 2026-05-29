using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI.Aggregates;

internal static class UserServicesExtensions
{
    internal static IServiceCollection AddUserAggregateServices(this IServiceCollection services)
    {
        // Repository
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
