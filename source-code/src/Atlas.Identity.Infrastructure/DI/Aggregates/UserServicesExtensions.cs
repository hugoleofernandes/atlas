using Atlas.Identity.Application.Queries.Users.ListUsers;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Infrastructure.Readers.Users.ListUsers;
using Atlas.Identity.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI.Aggregates;

internal static class UserServicesExtensions
{
    internal static IServiceCollection AddUserAggregateServices(this IServiceCollection services)
    {
        // Repository
        services.AddScoped<IUserRepository, UserRepository>();

        // Readers
        services.AddScoped<IListUsersReader, ListUsersReader>();

        // Query Handlers
        services.AddScoped<IListUsersQueryHandler, ListUsersQueryHandler>();

        return services;
    }
}
