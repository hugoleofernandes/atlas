using Atlas.Identity.Application.Tenants.Commands.SendWelcomeEmail;
using Atlas.Integration.Contracts.Tenants;
using Atlas.Outbox.Consumer.Identity.Tenants.UserCreatedFromInvitation.Identity;
using Atlas.Outbox.Consumer.Identity.Tenants.UserCreatedFromInvitation.Staff;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.Staff.Application.StaffMembers.Commands.CreateFromInvitation;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Consumer.Identity.Tenants.UserCreatedFromInvitation;

/// <summary>
/// Fan-out map for UserCreatedFromInvitationIntegrationEvent.
/// Every line in the consume section = one independent action triggered by this event.
///
///   Identity/ SendWelcomeEmailOnUserCreatedHandler   → SendWelcomeEmailCommand
///   Staff/    CreateStaffMemberOnUserCreatedHandler  → CreateStaffMemberFromInvitationCommand
///
/// To add a new action: create an adapter in the matching module subfolder and add one line below.
/// </summary>
internal static class UserCreatedFromInvitationConsumeDI
{
    internal static IServiceCollection AddUserCreatedFromInvitationConsumeHandlers(
        this IServiceCollection services)
    {
        // ── Fan-out ──────────────────────────────────────────────────────────
        services.AddScoped<
            IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>,
            SendWelcomeEmailOnUserCreatedHandler>();         // Identity

        services.AddScoped<
            IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>,
            CreateStaffMemberOnUserCreatedHandler>();        // Staff

        // ── Application handlers called by the adapters above ────────────────
        services.AddScoped<ISendWelcomeEmailCommandHandler,                 SendWelcomeEmailCommandHandler>();
        services.AddScoped<ICreateStaffMemberFromInvitationCommandHandler,  CreateStaffMemberFromInvitationCommandHandler>();

        return services;
    }
}
