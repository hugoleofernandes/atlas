using Atlas.Integration.Contracts.Tenants;
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
///   Staff module — CreateStaffMemberIntegrationEventHandler
///     Creates the corresponding StaffMember for the new user.
///
/// Note: the Identity-module handler (welcome e-mail) is registered by
/// Atlas.Identity.Integration via UserCreatedFromInvitationDependencyInjection.
/// It lives there because it owns the integration-contract → application-command
/// translation and should not be referenced from here.
/// </summary>
internal static class UserCreatedFromInvitation
{
    internal static IServiceCollection AddUserCreatedFromInvitationHandlers(
        this IServiceCollection services)
    {
        // Staff module — creates the StaffMember linked to the new user.
        services.AddScoped<
            IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>,
            CreateStaffMemberIntegrationEventHandler>();

        return services;
    }
}
