using Atlas.Staff.Domain.Entities;

namespace Atlas.Staff.Application.StaffMemberApp.Persistence;

public interface IStaffMemberRepository
{
    Task<StaffMember?> GetByIdAsync(Guid staffMemberId, CancellationToken ct);
    Task<bool> ExistsAsync(Guid tenantId, Guid userId, CancellationToken ct);
    Task<bool> ExistsForPartyAsync(Guid tenantId, Guid partyId, CancellationToken ct);
    Task AddAsync(StaffMember staffMember, CancellationToken ct);
}
