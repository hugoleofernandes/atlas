using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Domain.Entities.Tenants.Events;

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