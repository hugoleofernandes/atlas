using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Party.Domain.Parties.Events;

public sealed class IndividualRegisteredDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public Guid PartyId { get; }
    public string TaxNumber { get; }
    public string FullName { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public IndividualRegisteredDomainEvent(Guid tenantId, Guid partyId, string taxNumber, string fullName)
    {
        TenantId = tenantId;
        PartyId = partyId;
        TaxNumber = taxNumber;
        FullName = fullName;
    }
}
