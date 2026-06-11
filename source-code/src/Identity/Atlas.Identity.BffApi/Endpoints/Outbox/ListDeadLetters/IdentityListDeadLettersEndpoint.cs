using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Contracts.Permissions;
using Atlas.Outbox.Application.Queries.ListDeadLetters;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Outbox.ListDeadLetters;

public sealed class IdentityListDeadLettersEndpoint(
    IIdentityListDeadLettersQueryHandler handler,
    IHandlerInvoker invoker)
    : AtlasEndpoint<EmptyRequest, IReadOnlyList<DeadLetterSummary>>
{
    public override void Configure()
    {
        Get("bff/v1/outbox/identity/dead-letters");
        Policies($"permission:{IdentityModulePermissions.Outbox.Read}");
        Description(d => d.Produces<IReadOnlyList<DeadLetterSummary>>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new ListDeadLettersQuery(), ct);
        await OkFromResultAsync(result, ct);
    }
}
