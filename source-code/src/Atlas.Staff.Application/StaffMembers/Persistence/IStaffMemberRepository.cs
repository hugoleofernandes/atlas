using Atlas.Staff.Domain.Entities;

namespace Atlas.Staff.Application.StaffMemberApp.Persistence;

public interface IStaffMemberRepository
{
    Task<bool> ExistsAsync(
        Guid tenantId,
        Guid UserId,
        CancellationToken ct);

    Task AddAsync(
        StaffMember staff,
        CancellationToken ct);
}