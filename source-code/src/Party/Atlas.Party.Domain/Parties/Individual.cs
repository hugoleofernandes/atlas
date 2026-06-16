using Atlas.Party.Domain.Parties.Events;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Domain.Parties;

/// <summary>
/// A natural person (pessoa física) identified by CPF.
/// </summary>
public sealed class Individual : Party
{
    public PersonName Name { get; private set; } = default!;

    public DateOnly? BirthDate { get; private set; }

    public Gender? Gender { get; private set; }

    private Individual() { }

    private Individual(Guid tenantId, TaxNumber taxNumber, PersonName name, DateOnly? birthDate, Gender? gender)
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
    /// Registers a new individual and emits IndividualRegisteredDomainEvent.
    ///
    /// Pre-conditions (enforced by the caller):
    /// - TaxNumber (CPF) must not already be registered in this tenant.
    ///
    /// Emits: IndividualRegisteredDomainEvent
    /// </summary>
    public static Individual Register(
        Guid tenantId,
        TaxNumber taxNumber,
        PersonName name,
        DateOnly? birthDate = null,
        Gender? gender = null)
    {
        var individual = new Individual(tenantId, taxNumber, name, birthDate, gender);
        individual.AddDomainEvent(new IndividualRegisteredDomainEvent(tenantId, individual.Id, taxNumber.Value, name.FullName));
        return individual;
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
