using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Application.StaffMembers.Commands.Register;
using Atlas.Staff.Contracts.Permissions;
using Microsoft.AspNetCore.Http;

namespace Atlas.Staff.BffApi.Endpoints.StaffMembers.Register;

public sealed class RegisterStaffMemberEndpoint(
    IRegisterStaffMemberCommandHandler handler,
    IHandlerInvoker invoker
) : AtlasEndpoint<RegisterStaffMemberRequest, RegisterStaffMemberResponse>
{
    public override void Configure()
    {
        Post("bff/v1/staff/staff-members");
        Policies($"permission:{StaffModulePermissions.StaffMember.Create}");
        Description(d => d.Produces<RegisterStaffMemberResponse>(201));
    }

    public override async Task HandleAsync(RegisterStaffMemberRequest req, CancellationToken ct)
    {
        var command = new RegisterStaffMemberCommand(
            PartyId: req.PartyId,
            EmployeeNumber: req.EmployeeNumber,
            ContractType: req.ContractType,
            HireDate: req.HireDate
        );

        var result = await invoker.InvokeAsync(handler, command, ct);
        await CreatedFromResultAsync(result, RegisterStaffMemberResponse.From, ct);
    }
}
