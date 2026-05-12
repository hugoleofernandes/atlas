using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Domain.Entities.Tenants.Events;

public sealed class UserInvitedDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    //public Guid UserId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;


    public string Email { get; }
    
    //public string Role { get; }


    public UserInvitedDomainEvent(Guid tenantId, string email)//, Guid userId)//, string role)
    {
        TenantId = tenantId;
        Email = email;
        //UserId = userId;
        //Role = role;
    }
}