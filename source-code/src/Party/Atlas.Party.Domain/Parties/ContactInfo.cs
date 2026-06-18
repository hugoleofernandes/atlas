using Atlas.Party.Domain.Shared;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Parties;

/// <summary>
/// A contact entry belonging to a Party.
/// The ContactType determines how Value is interpreted (email, phone, free-text for Other).
/// At most one contact per ContactType can be flagged as primary.
/// </summary>
public sealed class ContactInfo : AuditableEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid PartyId { get; private set; }

    public ContactType Type { get; private set; }

    public string Value { get; private set; } = default!;

    public bool IsPrimary { get; private set; }

    private ContactInfo() { }

    internal static ContactInfo Create(Guid partyId, ContactType type, string value, bool isPrimary)
        => new() { PartyId = partyId, Type = type, Value = value, IsPrimary = isPrimary };

    internal void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;
}
