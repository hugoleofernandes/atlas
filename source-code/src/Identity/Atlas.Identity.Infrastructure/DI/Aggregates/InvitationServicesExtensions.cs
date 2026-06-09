using Atlas.Identity.Application.Commands.InviteUser;
using Atlas.Identity.Application.Queries.Invitations.ListInvitations;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Infrastructure.Readers.Invitations.ListInvitations;
using Atlas.Identity.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.DI.Aggregates;

internal static class InvitationServicesExtensions
{
    internal static IServiceCollection AddInvitationAggregateServices(this IServiceCollection services)
    {
        // Repository
        services.AddScoped<IInvitationRepository, InvitationRepository>();

        // Readers
        services.AddScoped<IListInvitationsReader, ListInvitationsReader>();

        // Query Handlers
        services.AddScoped<IListInvitationsQueryHandler, ListInvitationsQueryHandler>();

        // Command Handlers
        services.AddScoped<IInviteUserCommandHandler, InviteUserCommandHandler>();

        return services;
    }
}
