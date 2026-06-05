using Atlas.SharedKernel.Application.Logging;

namespace Atlas.Identity.Contracts.IntegrationEvents.Users;

/// <summary>
/// Public integration event published to the outbox when a tenant invitation is created.
/// Consumed by any module that needs to react to this fact
/// (e.g. Identity sends the invitation e-mail).
/// </summary>
public sealed record UserInvitedIntegrationEvent(Guid TenantId, string Email) : ILogSummary
{
    public string ToLogSummary() => $"TenantId={TenantId}";
}
