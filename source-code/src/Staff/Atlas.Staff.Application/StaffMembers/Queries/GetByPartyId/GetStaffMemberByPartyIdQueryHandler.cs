using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Staff.Application.StaffMembers.Queries.GetByPartyId;

public sealed class GetStaffMemberByPartyIdQueryHandler(
    IGetStaffMemberByPartyIdReader reader
) : IGetStaffMemberByPartyIdQueryHandler
{
    public Task<GetStaffMemberByPartyIdDto?> ExecuteAsync(
        GetStaffMemberByPartyIdQuery query,
        CancellationToken ct)
        => reader.FindAsync(query.PartyId, query.TenantId, ct);
}
