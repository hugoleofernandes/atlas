using Atlas.SharedDomain.Staff;
using Atlas.SharedKernel.Domain;

namespace Atlas.Staff.Domain.Entities;

public sealed class StaffMember : IAuditableAggregate
{
    public Guid EntityTypeId => StaffEntityTypes.StaffMember;

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public Guid UserId { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string Role { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private StaffMember() { }

    public StaffMember(
        Guid tenantId,
        Guid UserId,
        string firstName,
        string lastName,
        string role)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = UserId;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}