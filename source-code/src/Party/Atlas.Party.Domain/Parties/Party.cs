using Atlas.Party.Domain.Parties.Events;
using Atlas.Party.Domain.Parties.Exceptions;
using Atlas.Party.Domain.Shared;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Parties;

/// <summary>
/// Abstract root of the Party hierarchy.
/// Represents any entity (person or organization) that can participate in business relationships.
/// Concrete types: Individual (CPF) and Organization (CNPJ).
///
/// Invariants:
/// - TaxNumber is unique per tenant.
/// - A deactivated party cannot be deactivated again.
/// - Address and ContactInfo collections are managed through Party's methods only.
/// </summary>
public abstract class Party : AggregateRoot, IMultiTenantEntity, IAuditableAggregate
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

    // =========================
    // CONTACTS
    // =========================

    /// <summary>
    /// Adds an email contact entry. When isPrimary is true any existing primary email is demoted.
    /// </summary>
    public ContactInfo AddEmailContact(EmailAddress email, ContactType type = ContactType.Email, bool isPrimary = false)
    {
        if (isPrimary)
            DemotePrimaryContacts(c => c.Email is not null);

        var contact = ContactInfo.ForEmail(Id, email, type, isPrimary);
        _contacts.Add(contact);
        return contact;
    }

    /// <summary>
    /// Adds a phone contact entry. When isPrimary is true any existing primary phone is demoted.
    /// </summary>
    public ContactInfo AddPhoneContact(PhoneNumber phone, ContactType type = ContactType.Mobile, bool isPrimary = false)
    {
        if (isPrimary)
            DemotePrimaryContacts(c => c.Phone is not null);

        var contact = ContactInfo.ForPhone(Id, phone, type, isPrimary);
        _contacts.Add(contact);
        return contact;
    }

    private void DemotePrimaryContacts(Func<ContactInfo, bool> predicate)
    {
        foreach (var c in _contacts.Where(c => c.IsPrimary && predicate(c)))
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
