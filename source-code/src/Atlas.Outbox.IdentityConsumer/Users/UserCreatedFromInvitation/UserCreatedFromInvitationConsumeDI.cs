using System;
using System.Collections.Generic;
using System.Text;
using Atlas.Identity.Application.Commands.SendWelcomeEmail;
using Atlas.Integration.Contracts.Tenants;
using Atlas.Outbox.IdentityConsumer.Users.UserCreatedFromInvitation.Identity;
using Atlas.Outbox.IdentityConsumer.Users.UserCreatedFromInvitation.Staff;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.Staff.Application.StaffMembers.Commands.CreateFromInvitation;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.IdentityConsumer.Users.UserCreatedFromInvitation;

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
    internal static IServiceCollection AddUserCreatedFromInvitationConsumeHandlers(this IServiceCollection services)
    {
        // ── Fan-out ──────────────────────────────────────────────────────────
        services.AddScoped<
            IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>,
            SendWelcomeEmailOnUserCreatedHandler
        >(); // Identity

        //services.AddScoped<
        //    IIntegrationEventHandler<UserCreatedFromInvitationIntegrationEvent>,
        //    CreateStaffMemberOnUserCreatedHandler
        //>(); // Staff
        //todo: I need to revise the code above.
        //I don't want to inject two handlers for the same event, because they will run in an undefined order.
        //I need to create a single handler that calls both actions in the right order.
        //This is important because the staff member needs to be created before the welcome email is sent, otherwise the email will fail when it tries to get staff details.

        // ── Application handlers called by the adapters above ────────────────
        services.AddScoped<ISendWelcomeEmailCommandHandler, SendWelcomeEmailCommandHandler>();
        services.AddScoped<
            ICreateStaffMemberFromInvitationCommandHandler,
            CreateStaffMemberFromInvitationCommandHandler
        >();

        return services;
    }
}
