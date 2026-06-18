using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Application.StaffMembers.Queries.GetById;
using Atlas.Staff.Contracts.Permissions;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Staff.BffApi.Endpoints.StaffMembers.GetById;

public sealed record GetStaffMemberByIdRequest(Guid Id);

public sealed record GetStaffMemberByIdResponse(
    Guid StaffMemberId,
    Guid? PartyId,
    string? EmployeeNumber,
    string? ContractType,
    string? HireDate,
    string? TerminationDate,
    string Status,
    DateTime CreatedAt
)
{
    public static GetStaffMemberByIdResponse From(GetStaffMemberByIdDto dto)
        => new(
            StaffMemberId:   dto.StaffMemberId,
            PartyId:         dto.PartyId,
            EmployeeNumber:  dto.EmployeeNumber,
            ContractType:    dto.ContractType,
            HireDate:        dto.HireDate,
            TerminationDate: dto.TerminationDate,
            Status:          dto.Status,
            CreatedAt:       dto.CreatedAt
        );
}

public sealed class GetStaffMemberByIdEndpoint(
    IGetStaffMemberByIdQueryHandler handler,
    IHandlerInvoker invoker
) : AtlasEndpoint<GetStaffMemberByIdRequest, GetStaffMemberByIdResponse>
{
    public override void Configure()
    {
        Get("bff/v1/staff/staff-members/{id}");
        Policies($"permission:{StaffModulePermissions.StaffMember.Read}");
        Description(d => d.Produces<GetStaffMemberByIdResponse>());
    }

    public override async Task HandleAsync(GetStaffMemberByIdRequest req, CancellationToken ct)
    {
        var query = new GetStaffMemberByIdQuery(req.Id);
        var result = await invoker.InvokeAsync(handler, query, ct);
        var response = result.Map(GetStaffMemberByIdResponse.From);
        await OkFromResultAsync(response, ct);
    }
}
