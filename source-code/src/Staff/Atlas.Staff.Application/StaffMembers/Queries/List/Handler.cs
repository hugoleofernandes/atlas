using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Domain.Shared.Exceptions;

namespace Atlas.Staff.Application.StaffMembers.Queries.List;

public sealed class ListStaffMembersQueryHandler(
    IRequestContext requestContext,
    IListStaffMembersReader reader
) : IListStaffMembersQueryHandler
{
    public async Task<IReadOnlyList<ListStaffMembersDto>> ExecuteAsync(
        ListStaffMembersQuery query,
        CancellationToken ct)
    {
        var tenantId = requestContext.TenantId ?? throw new TenantContextNotResolvedException();
        return await reader.ListAsync(tenantId, ct);
    }
}
