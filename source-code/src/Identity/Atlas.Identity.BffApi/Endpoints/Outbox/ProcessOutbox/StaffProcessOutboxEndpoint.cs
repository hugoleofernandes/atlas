using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Outbox.Application.Commands.ProcessOutbox;
using Atlas.Outbox.Application.Workflows.OutboxProcessing;
using Atlas.Staff.Contracts.Permissions;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Outbox.ProcessOutbox;

public sealed class StaffProcessOutboxEndpoint(IStaffOutboxProcessingWorkflow workflow)
    : AtlasEndpoint<EmptyRequest, EmptyResponse>
{
    public override void Configure()
    {
        Post("bff/v1/outbox/staff/process");
        Policies($"permission:{StaffModulePermissions.Outbox.Process}");
        Description(d => d.Produces(204));
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var command = new ProcessOutboxCommand(
            BatchSize: 50,
            MaxRetries: 5,
            LockDuration: TimeSpan.FromSeconds(30),
            Module: "staff"
        );

        await workflow.RunAsync(command, ct);
        await Send.NoContentAsync(ct);
    }
}
