using Atlas.Contracts.Tenants.IntegrationEvents;
using Atlas.Identity.Application.Tenants.Services.IntegrationEventHandlers;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.Staff.Application.IntegrationEventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Integration.Tenants;

/// <summary>
/// Fan-out map for <see cref="UserCreatedFromInvitationIntegrationEvent"/>.
///
/// When the Outbox Worker dequeues this event, every handler registered here
/// is invoked independently — a failure in one does not prevent the others.
///
///   Identity module — UserCreatedFromInvitationIntegrationEventHandler
///     Creates the user record on the Identity side when an invitation is accepted.
///
///   Staff module — CreateStaffMemberIntegrationEventHandler
///     Creates the corresponding StaffMember for the new user.
/// </summary>
internal static class UserCreatedFromInvitation
{
    internal static IServiceCollection AddUserCreatedFromInvitationHandlers(
        this IServiceCollection services)
    {
        // Identity module — persists the user record on the Identity side.
        services.AddScoped<
            IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>,
            UserCreatedFromInvitationIntegrationEventHandler>();

        // Staff module — creates the StaffMember linked to the new user.
        services.AddScoped<
            IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>,
            CreateStaffMemberIntegrationEventHandler>();

        return services;
    }
}
