using Atlas.Party.Domain.Shared;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Parties;

/// <summary>
/// A contact entry (email or phone) belonging to a Party.
/// Exactly one email and one phone can be flagged as primary at any time.
/// </summary>
public sealed class ContactInfo : AuditableEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid PartyId { get; private set; }

    public ContactType Type { get; private set; }

    /// <summary>Email address — populated when Type is Email.</summary>
    public EmailAddress? Email { get; private set; }

    /// <summary>Phone number — populated when Type is Phone, Mobile, or WhatsApp.</summary>
    public PhoneNumber? Phone { get; private set; }

    public bool IsPrimary { get; private set; }

    private ContactInfo() { }

    internal static ContactInfo ForEmail(Guid partyId, EmailAddress email, ContactType type, bool isPrimary)
        => new() { PartyId = partyId, Type = type, Email = email, IsPrimary = isPrimary };

    internal static ContactInfo ForPhone(Guid partyId, PhoneNumber phone, ContactType type, bool isPrimary)
        => new() { PartyId = partyId, Type = type, Phone = phone, IsPrimary = isPrimary };

    internal void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;
}
