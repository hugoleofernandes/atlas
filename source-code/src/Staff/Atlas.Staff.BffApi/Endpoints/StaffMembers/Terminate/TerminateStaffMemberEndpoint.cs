using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Application.StaffMembers.Commands.Terminate;
using Atlas.Staff.Contracts.Permissions;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Staff.BffApi.Endpoints.StaffMembers.Terminate;

public sealed class TerminateStaffMemberEndpoint(
    ITerminateStaffMemberCommandHandler handler,
    IHandlerInvoker invoker
) : AtlasEndpoint<TerminateStaffMemberRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("bff/v1/staff/staff-members/{id}");
        Policies($"permission:{StaffModulePermissions.StaffMember.Deactivate}");
        Description(d => d.Produces(204));
    }

    public override async Task HandleAsync(TerminateStaffMemberRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var command = new TerminateStaffMemberCommand(
            StaffMemberId: id,
            TerminationDate: req.TerminationDate
        );

        var result = await invoker.InvokeAsync(handler, command, ct);
        await DeletedFromResultAsync(result, ct);
    }
}
