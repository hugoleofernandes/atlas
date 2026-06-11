using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Outbox.Application.Commands.ResubmitDeadLetter;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Contracts.Permissions;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Outbox.ResubmitDeadLetter;

public sealed class StaffResubmitDeadLetterEndpoint(
    IStaffResubmitDeadLetterCommandHandler handler,
    IHandlerInvoker invoker)
    : AtlasEndpoint<ResubmitDeadLetterRequest, ResubmitDeadLetterResponse>
{
    public override void Configure()
    {
        Post("bff/v1/outbox/staff/dead-letters/{Id}/resubmit");
        Policies($"permission:{StaffModulePermissions.Outbox.Resubmit}");
        Description(d => d.Produces<ResubmitDeadLetterResponse>(200));
    }

    public override async Task HandleAsync(ResubmitDeadLetterRequest req, CancellationToken ct)
    {
        var cmd = new ResubmitDeadLetterCommand(req.Id);
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await UpdatedFromResultAsync(result, ResubmitDeadLetterResponse.From, ct);
    }
}
