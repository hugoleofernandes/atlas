using Atlas.Identity.Contracts.IntegrationEvents.Users;
using Atlas.Outbox.Application.Targets;
using Atlas.Outbox.Domain.Targets;
using Atlas.Outbox.Domain.Targets.Names;

namespace Atlas.Outbox.Catalog.Identity;

/// <summary>
/// Direct targets triggered by the Identity-originated
/// UserInvitedIntegrationEvent.
/// </summary>
public sealed class UserInvitedTargetCatalog : ITargetCatalog
{
    private static readonly IReadOnlyList<TargetMapping> Targets =
    [
        new TargetMapping(IdentityTargetNames.IdentitySendInvitationEmail, TargetMode.Direct, Order: 10),
    ];

    public IReadOnlyList<TargetMapping> GetFor(Type eventType) =>
        eventType == typeof(UserInvitedIntegrationEvent) ? Targets : [];
}
