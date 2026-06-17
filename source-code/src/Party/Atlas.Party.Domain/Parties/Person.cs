using Atlas.Party.Domain.Parties.Events;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Domain.Parties;

/// <summary>
/// A natural person (pessoa física) identified by CPF.
/// </summary>
public sealed class Person : Party
{
    public PersonName Name { get; private set; } = default!;

    public DateOnly? BirthDate { get; private set; }

    public Gender? Gender { get; private set; }

    private Person() { }

    private Person(Guid tenantId, TaxNumber taxNumber, PersonName name, DateOnly? birthDate, Gender? gender)
    {
        TenantId = tenantId;
        TaxNumber = taxNumber;
        Name = name;
        BirthDate = birthDate;
        Gender = gender;
    }

    // =========================
    // FACTORY
    // =========================

    /// <summary>
    /// Registers a new person and emits PersonRegisteredDomainEvent.
    ///
    /// Pre-conditions (enforced by the caller):
    /// - TaxNumber (CPF) must not already be registered in this tenant.
    ///
    /// Emits: PersonRegisteredDomainEvent
    /// </summary>
    public static Person Register(
        Guid tenantId,
        TaxNumber taxNumber,
        PersonName name,
        DateOnly? birthDate = null,
        Gender? gender = null)
    {
        var person = new Person(tenantId, taxNumber, name, birthDate, gender);
        person.AddDomainEvent(new PersonRegisteredDomainEvent(tenantId, person.Id, taxNumber.Value, name.FullName));
        return person;
    }

    // =========================
    // BEHAVIOUR
    // =========================

    /// <summary>Updates mutable personal details. Does not change TaxNumber or TenantId.</summary>
    public void Update(PersonName name, DateOnly? birthDate, Gender? gender)
    {
        Name = name;
        BirthDate = birthDate;
        Gender = gender;
    }
}

