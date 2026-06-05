using Atlas.SharedKernel.Application.Logging;

namespace Atlas.Identity.Contracts.IntegrationEvents.Users;

/// <summary>
/// Public integration event published to the outbox when a user completes
/// registration via an invitation link.
/// Consumed by any module that needs to react to this fact
/// (e.g. Identity sends a welcome email, Staff creates a StaffMember).
/// </summary>
public sealed record UserCreatedFromInvitationIntegrationEvent(Guid TenantId, Guid UserId, string Email, string Role)
    : ILogSummary
{
    /// <summary>
    /// PII-safe summary for Information-level logs and Loki.
    /// Email is intentionally excluded — identifiers only.
    /// </summary>
    public string ToLogSummary() => $"TenantId={TenantId} UserId={UserId} Role={Role}";
}
