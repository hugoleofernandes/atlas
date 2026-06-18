using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Domain.Entities.Exceptions;
using Atlas.Staff.Domain.Shared.Exceptions;

namespace Atlas.Staff.Application.StaffMembers.Queries.GetById;

public sealed class GetStaffMemberByIdQueryHandler(
    IRequestContext requestContext,
    IGetStaffMemberByIdReader reader
) : IGetStaffMemberByIdQueryHandler
{
    public async Task<GetStaffMemberByIdDto> ExecuteAsync(
        GetStaffMemberByIdQuery query,
        CancellationToken ct)
    {
        var tenantId = requestContext.TenantId ?? throw new TenantContextNotResolvedException();
        return await reader.FindAsync(query.StaffMemberId, tenantId, ct)
            ?? throw new StaffMemberNotFoundException(query.StaffMemberId);
    }
}
