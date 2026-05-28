using Atlas.Identity.Application.Aggregates.Invitations;
using Atlas.Identity.Application.Aggregates.Invitations.Handlers.Commands.InviteUser;
using Atlas.Identity.Application.Aggregates.Invitations.Handlers.Queries.ListInvitations;
using Atlas.Identity.Infrastructure.Aggregates.Invitations.Readers.ListInvitations;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.Aggregates.Invitations;

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
