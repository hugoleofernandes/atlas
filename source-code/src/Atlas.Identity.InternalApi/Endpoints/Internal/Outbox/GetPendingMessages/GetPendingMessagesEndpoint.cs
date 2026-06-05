using Atlas.BuildingBlocks.AspNetCore.Security.InternalApi;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Outbox.Application.Queries.GetPendingMessages;
using Atlas.Outbox.Contracts;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.InternalApi.Endpoints.Internal.Outbox.GetPendingMessages;

/// <summary>
/// Returns a locked batch of pending outbox messages for the Identity module.
/// Used by the outbox worker when running in Http dispatch mode.
/// </summary>
public sealed class GetPendingMessagesEndpoint(
    IGetPendingMessagesQueryHandler handler,
    IHandlerInvoker invoker)
    : AtlasEndpoint<GetPendingMessagesRequest, IReadOnlyList<OutboxMessageDto>>
{
    public override void Configure()
    {
        Get("internal/identity/outbox/pending-messages");
        Policies(InternalApiKeyDefaults.PolicyName);
        Description(d => d.Produces<IReadOnlyList<OutboxMessageDto>>(200));
    }

    public override async Task HandleAsync(GetPendingMessagesRequest req, CancellationToken ct)
    {
        var query  = new GetPendingMessagesQuery(
            req.BatchSize,
            TimeSpan.FromSeconds(req.LockDurationSeconds));

        var result = await invoker.InvokeAsync(handler, query, ct);
        await OkFromResultAsync(result, ct);
    }
}
