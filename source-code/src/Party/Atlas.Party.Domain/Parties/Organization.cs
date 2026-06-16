using Atlas.Party.Domain.Parties.Events;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Domain.Parties;

/// <summary>
/// A legal entity (pessoa jurídica) identified by CNPJ.
/// </summary>
public sealed class Organization : Party
{
    /// <summary>Official registered name (razão social).</summary>
    public string LegalName { get; private set; } = default!;

    /// <summary>Commercial name (nome fantasia). Optional.</summary>
    public string? TradeName { get; private set; }

    public LegalType LegalType { get; private set; }

    private Organization() { }

    private Organization(Guid tenantId, TaxNumber taxNumber, string legalName, string? tradeName, LegalType legalType)
    {
        TenantId = tenantId;
        TaxNumber = taxNumber;
        LegalName = legalName;
        TradeName = tradeName;
        LegalType = legalType;
    }

    // =========================
    // FACTORY
    // =========================

    /// <summary>
    /// Registers a new organization and emits OrganizationRegisteredDomainEvent.
    ///
    /// Pre-conditions (enforced by the caller):
    /// - TaxNumber (CNPJ) must not already be registered in this tenant.
    ///
    /// Emits: OrganizationRegisteredDomainEvent
    /// </summary>
    public static Organization Register(
        Guid tenantId,
        TaxNumber taxNumber,
        string legalName,
        string? tradeName,
        LegalType legalType)
    {
        var org = new Organization(tenantId, taxNumber, legalName, tradeName, legalType);
        org.AddDomainEvent(new OrganizationRegisteredDomainEvent(tenantId, org.Id, taxNumber.Value, legalName));
        return org;
    }

    // =========================
    // BEHAVIOUR
    // =========================

    /// <summary>Updates mutable company details. Does not change TaxNumber or TenantId.</summary>
    public void Update(string legalName, string? tradeName, LegalType legalType)
    {
        LegalName = legalName;
        TradeName = tradeName;
        LegalType = legalType;
    }
}
