using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Application.StaffMembers.Commands.Update;
using Atlas.Staff.Contracts.Permissions;
using Microsoft.AspNetCore.Http;

namespace Atlas.Staff.BffApi.Endpoints.StaffMembers.Update;

public sealed class UpdateStaffMemberEndpoint(
    IUpdateStaffMemberCommandHandler handler,
    IHandlerInvoker invoker
) : AtlasEndpoint<UpdateStaffMemberRequest, UpdateStaffMemberResponse>
{
    public override void Configure()
    {
        Put("bff/v1/staff/staff-members/{id}");
        Policies($"permission:{StaffModulePermissions.StaffMember.Update}");
        Description(d => d.Produces<UpdateStaffMemberResponse>());
    }

    public override async Task HandleAsync(UpdateStaffMemberRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var command = new UpdateStaffMemberCommand(
            StaffMemberId: id,
            ContractType: req.ContractType,
            HireDate: req.HireDate
        );

        var result = await invoker.InvokeAsync(handler, command, ct);
        await UpdatedFromResultAsync(result, UpdateStaffMemberResponse.From, ct);
    }
}
