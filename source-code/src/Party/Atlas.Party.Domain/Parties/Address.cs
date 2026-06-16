using Atlas.Party.Domain.Shared;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Parties;

/// <summary>
/// A postal address belonging to a Party.
/// A Party may have multiple addresses; at most one per type can be marked as primary.
/// </summary>
public sealed class Address : AuditableEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid PartyId { get; private set; }

    public AddressType Type { get; private set; }

    public PostalAddress PostalAddress { get; private set; } = default!;

    public bool IsPrimary { get; private set; }

    private Address() { }

    internal Address(Guid partyId, AddressType type, PostalAddress postalAddress, bool isPrimary)
    {
        PartyId = partyId;
        Type = type;
        PostalAddress = postalAddress;
        IsPrimary = isPrimary;
    }

    internal void Update(AddressType type, PostalAddress postalAddress, bool isPrimary)
    {
        Type = type;
        PostalAddress = postalAddress;
        IsPrimary = isPrimary;
    }

    internal void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;
}
