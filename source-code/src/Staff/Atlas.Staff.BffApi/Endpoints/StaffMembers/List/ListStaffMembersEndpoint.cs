using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Application.StaffMembers.Queries.List;
using Atlas.Staff.Contracts.Permissions;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Staff.BffApi.Endpoints.StaffMembers.List;

public sealed record ListStaffMembersRequest;

public sealed record StaffMemberListItem(
    Guid StaffMemberId,
    Guid? PartyId,
    string? EmployeeNumber,
    string? ContractType,
    string Status,
    string? HireDate
)
{
    public static StaffMemberListItem From(ListStaffMembersDto dto)
        => new(
            StaffMemberId:  dto.StaffMemberId,
            PartyId:        dto.PartyId,
            EmployeeNumber: dto.EmployeeNumber,
            ContractType:   dto.ContractType,
            Status:         dto.Status,
            HireDate:       dto.HireDate
        );
}

public sealed class ListStaffMembersEndpoint(
    IListStaffMembersQueryHandler handler,
    IHandlerInvoker invoker
) : AtlasEndpoint<ListStaffMembersRequest, IReadOnlyList<StaffMemberListItem>>
{
    public override void Configure()
    {
        Get("bff/v1/staff/staff-members");
        Policies($"permission:{StaffModulePermissions.StaffMember.Read}");
        Description(d => d.Produces<IReadOnlyList<StaffMemberListItem>>());
    }

    public override async Task HandleAsync(ListStaffMembersRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new ListStaffMembersQuery(), ct);
        var response = result.Map(items => (IReadOnlyList<StaffMemberListItem>)items.Select(StaffMemberListItem.From).ToList());
        await OkFromResultAsync(response, ct);
    }
}
