using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Contracts.Permissions;
using Atlas.Outbox.Application.Commands.ProcessOutbox;
using Atlas.Outbox.Application.Workflows.OutboxProcessing;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Outbox.ProcessOutbox;

public sealed class IdentityProcessOutboxEndpoint(IIdentityOutboxProcessingWorkflow workflow)
    : AtlasEndpoint<EmptyRequest, EmptyResponse>
{
    public override void Configure()
    {
        Post("bff/v1/outbox/identity/process");
        Policies($"permission:{IdentityModulePermissions.Outbox.Process}");
        Description(d => d.Produces(204));
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var command = new ProcessOutboxCommand(
            BatchSize: 50,
            MaxRetries: 5,
            LockDuration: TimeSpan.FromSeconds(30),
            Module: "identity"
        );

        await workflow.RunAsync(command, ct);
        await Send.NoContentAsync(ct);
    }
}
