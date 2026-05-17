namespace Atlas.Contracts.Tenants.IntegrationEvents;

/// <summary>
/// Public integration event published to the outbox when a user completes
/// registration via an invitation link.
/// Consumed by any module that needs to react to this fact
/// (e.g. Identity sends a welcome email, Staff creates a StaffMember).
/// </summary>
public sealed record UserCreatedFromInvitationIntegrationEvent(
    Guid TenantId,
    Guid UserId,
    string Email,
    string Role
);
