using Atlas.Party.Domain.Parties.Events;
using Atlas.Party.Domain.Parties.Exceptions;
using Atlas.Party.Domain.Shared;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Parties;

/// <summary>
/// Abstract root of the Party hierarchy.
/// Represents any entity (person or organization) that can participate in business relationships.
/// Concrete types: Person (CPF) and Organization (CNPJ).
///
/// Invariants:
/// - TaxNumber is unique per tenant.
/// - A deactivated party cannot be deactivated again.
/// - Address and ContactInfo collections are managed through Party's methods only.
/// </summary>
public abstract class Party : AggregateRoot, IMultiTenantEntity
{
    private readonly List<Address> _addresses = new();
    private readonly List<ContactInfo> _contacts = new();

    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid TenantId { get; protected set; }

    public TaxNumber TaxNumber { get; protected set; } = default!;

    public bool IsActive { get; private set; } = true;

    public IReadOnlyList<Address> Addresses => _addresses;
    public IReadOnlyList<ContactInfo> Contacts => _contacts;

    void IMultiTenantEntity.SetTenantId(Guid tenantId) => TenantId = tenantId;

    protected Party() { }

    // =========================
    // ADDRESSES
    // =========================

    /// <summary>
    /// Adds an address. When isPrimary is true any existing primary address of the same type is demoted.
    /// </summary>
    public Address AddAddress(AddressType type, PostalAddress postalAddress, bool isPrimary = false)
    {
        if (isPrimary)
            DemotePrimaryAddresses(type);

        var address = new Address(Id, type, postalAddress, isPrimary);
        _addresses.Add(address);
        return address;
    }

    private void DemotePrimaryAddresses(AddressType type)
    {
        foreach (var a in _addresses.Where(a => a.Type == type && a.IsPrimary))
            a.SetPrimary(false);
    }

    /// <summary>
    /// Replaces the entire address collection. Intended for flows where the caller manages
    /// the full address list client-side (add/remove without round-tripping) and submits it
    /// atomically together with the rest of the Party on save.
    /// </summary>
    public void ReplaceAddresses(IReadOnlyList<AddressInput> addresses)
    {
        foreach (var group in addresses.GroupBy(a => a.Type))
            if (group.Count(a => a.IsPrimary) > 1)
                throw new MultiplePrimaryAddressesException(group.Key);

        _addresses.Clear();
        foreach (var a in addresses)
            _addresses.Add(new Address(Id, a.Type, a.PostalAddress, a.IsPrimary));
    }

    // =========================
    // CONTACTS
    // =========================

    /// <summary>
    /// Adds a contact entry. When isPrimary is true any existing primary contact of the same type is demoted.
    /// </summary>
    public ContactInfo AddContact(ContactType type, string value, bool isPrimary = false)
    {
        if (isPrimary)
            DemotePrimaryContacts(type);

        var contact = ContactInfo.Create(Id, type, value, isPrimary);
        _contacts.Add(contact);
        return contact;
    }

    /// <summary>
    /// Replaces the entire contact collection. Intended for flows where the caller manages
    /// the full contact list client-side and submits it atomically with the rest of the Party.
    /// </summary>
    public void ReplaceContacts(IReadOnlyList<ContactInput> contacts)
    {
        foreach (var group in contacts.GroupBy(c => c.Type))
            if (group.Count(c => c.IsPrimary) > 1)
                throw new MultiplePrimaryContactsException(group.Key.ToString());

        _contacts.Clear();
        foreach (var c in contacts)
            _contacts.Add(ContactInfo.Create(Id, c.Type, c.Value, c.IsPrimary));
    }

    private void DemotePrimaryContacts(ContactType type)
    {
        foreach (var c in _contacts.Where(c => c.Type == type && c.IsPrimary))
            c.SetPrimary(false);
    }

    // =========================
    // LIFECYCLE
    // =========================

    /// <summary>
    /// Deactivates the party. Emits PartyDeactivatedDomainEvent.
    ///
    /// Downstream consumers (Staff, Customer, Supplier) are notified via integration event
    /// so they can take appropriate action (e.g. suspend access).
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new PartyAlreadyDeactivatedException(Id);

        IsActive = false;
        AddDomainEvent(new PartyDeactivatedDomainEvent(TenantId, Id));
    }
}
