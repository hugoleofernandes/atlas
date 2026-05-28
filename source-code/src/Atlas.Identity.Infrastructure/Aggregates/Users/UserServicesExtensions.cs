using Atlas.Identity.Application.Aggregates.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.Aggregates.Users;

internal static class UserServicesExtensions
{
    internal static IServiceCollection AddUserAggregateServices(this IServiceCollection services)
    {
        // Repository
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
