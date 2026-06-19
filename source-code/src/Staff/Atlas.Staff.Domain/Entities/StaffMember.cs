using Atlas.SharedKernel.Domain;
using Atlas.Staff.Domain.Entities.Exceptions;
using Atlas.Staff.Domain.Shared;

namespace Atlas.Staff.Domain.Entities;

/// <summary>
/// Represents an active employment relationship between a Party (Person) and a tenant.
/// Carries employment data — contract type, hire/termination dates, and status lifecycle.
/// </summary>
public sealed class StaffMember : AggregateRoot
{
    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    // ── legacy fields — kept for CreateStaffMemberFromInvitation outbox flow ──
    public Guid UserId { get; private set; }
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Role { get; private set; } = default!;
    public bool IsActive { get; private set; }
    // ──────────────────────────────────────────────────────────────────────────

    public Guid? PartyId { get; private set; }
    public string? EmployeeNumber { get; private set; }
    public ContractType? ContractType { get; private set; }
    public DateOnly? HireDate { get; private set; }
    public DateOnly? TerminationDate { get; private set; }
    public StaffStatus Status { get; private set; }

    private StaffMember() { }

    /// <summary>Legacy constructor used by the CreateStaffMemberFromInvitation outbox flow.</summary>
    public StaffMember(Guid tenantId, Guid userId, string firstName, string lastName, string role)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
        IsActive = true;
        Status = StaffStatus.Active;
    }

    /// <summary>Creates a new employment record linked to a Party (Person).</summary>
    public static StaffMember Register(
        Guid tenantId,
        Guid partyId,
        string employeeNumber,
        ContractType contractType,
        DateOnly hireDate)
    {
        return new StaffMember
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartyId = partyId,
            EmployeeNumber = employeeNumber,
            ContractType = contractType,
            HireDate = hireDate,
            Status = StaffStatus.Active,
            IsActive = true,
        };
    }

    public void Update(ContractType contractType, DateOnly hireDate)
    {
        ContractType = contractType;
        HireDate = hireDate;
    }

    public void Terminate(DateOnly terminationDate)
    {
        if (Status == StaffStatus.Terminated)
            throw new StaffMemberAlreadyTerminatedException(Id);

        Status = StaffStatus.Terminated;
        IsActive = false;
        TerminationDate = terminationDate;
    }

    public void Suspend()
    {
        Status = StaffStatus.Suspended;
        IsActive = false;
    }

    public void ReturnFromLeave()
    {
        Status = StaffStatus.Active;
        IsActive = true;
    }

    /// <summary>Legacy deactivate — kept for backward compatibility.</summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>Legacy profile update — kept for backward compatibility.</summary>
    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}
