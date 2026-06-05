using Atlas.Identity.Application.Commands.SendWelcomeEmail;
using Atlas.Integration.Contracts.Tenants;
using Atlas.Outbox.Application.DirectTargets;
using Atlas.Staff.Application.StaffMembers.Commands.CreateFromInvitation;

namespace Atlas.Outbox.DirectTargets.IdentityEvents.UserCreatedFromInvitation;

/// <summary>
/// Direct targets triggered by the Identity-originated
/// UserCreatedFromInvitationIntegrationEvent.
/// Grouping the full fan-out under the event folder makes it easy to answer:
/// "what does this event do?"
/// </summary>
public sealed class UserCreatedFromInvitationDirectTargetCatalog : IDirectOutboxTargetCatalog
{
    public const string StaffCreateMemberFromInvitation = "staff.create-member-from-invitation";
    public const string IdentitySendWelcomeEmail = "identity.send-welcome-email";

    private static readonly IReadOnlyList<DirectOutboxTargetDefinition> Targets =
    [
        new DirectOutboxTargetDefinition(
            typeof(UserCreatedFromInvitationIntegrationEvent),
            StaffCreateMemberFromInvitation,
            typeof(ICreateStaffMemberFromInvitationCommandHandler),
            Order: 10),
        new DirectOutboxTargetDefinition(
            typeof(UserCreatedFromInvitationIntegrationEvent),
            IdentitySendWelcomeEmail,
            typeof(ISendWelcomeEmailCommandHandler),
            Order: 20),
    ];

    public IReadOnlyList<DirectOutboxTargetDefinition> GetFor(Type eventType) =>
        eventType == typeof(UserCreatedFromInvitationIntegrationEvent)
            ? Targets
            : [];
}
