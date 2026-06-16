using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Party.Domain.Parties.Events;

public sealed class PartyDeactivatedDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public Guid PartyId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public PartyDeactivatedDomainEvent(Guid tenantId, Guid partyId)
    {
        TenantId = tenantId;
        PartyId = partyId;
    }
}
