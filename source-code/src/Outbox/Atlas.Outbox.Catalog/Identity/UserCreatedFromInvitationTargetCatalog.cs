using Atlas.Identity.Contracts.IntegrationEvents.Users;
using Atlas.Outbox.Application.Targets;
using Atlas.Outbox.Domain.Targets;
using Atlas.Outbox.Domain.Targets.Names;

namespace Atlas.Outbox.Catalog.Identity;

/// <summary>
/// Direct targets triggered by the Identity-originated
/// UserCreatedFromInvitationIntegrationEvent.
/// Grouping the full fan-out under the event folder makes it easy to answer:
/// "what does this event do?"
/// </summary>
public sealed class UserCreatedFromInvitationTargetCatalog : ITargetCatalog
{
    private static readonly IReadOnlyList<TargetMapping> Targets =
    [
        new TargetMapping(StaffTargetNames.StaffCreateMemberFromInvitation, TargetMode.Direct, Order: 10),
        new TargetMapping(IdentityTargetNames.IdentitySendWelcomeEmail, TargetMode.Direct, Order: 20),
    ];

    public IReadOnlyList<TargetMapping> GetFor(Type eventType) =>
        eventType == typeof(UserCreatedFromInvitationIntegrationEvent) ? Targets : [];
}
