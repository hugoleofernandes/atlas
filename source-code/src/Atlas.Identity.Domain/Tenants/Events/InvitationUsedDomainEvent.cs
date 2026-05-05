using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants.Events;

public sealed class InvitationUsedDomainEvent : DomainEvent
{
    public Guid TenantId { get; }
    public Guid InvitationId { get; }
    public string Email { get; }

    public InvitationUsedDomainEvent(Guid tenantId, Guid invitationId, string email)
    {
        TenantId = tenantId;
        InvitationId = invitationId;
        Email = email;
    }
}