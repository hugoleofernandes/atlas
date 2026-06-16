using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Party.Domain.Parties.Events;

public sealed class OrganizationRegisteredDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public Guid PartyId { get; }
    public string TaxNumber { get; }
    public string LegalName { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public OrganizationRegisteredDomainEvent(Guid tenantId, Guid partyId, string taxNumber, string legalName)
    {
        TenantId = tenantId;
        PartyId = partyId;
        TaxNumber = taxNumber;
        LegalName = legalName;
    }
}
