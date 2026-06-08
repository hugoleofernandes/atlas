using Atlas.BuildingBlocks.AspNetCore.Security.InternalApi;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.BuildingBlocks.Outbox.ListPendingMessages;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.InternalApi.Endpoints.Internal.Outbox.GetPendingMessages;

/// <summary>
/// Returns a locked batch of pending outbox messages for the Identity module.
/// Used by the outbox worker when running in Http dispatch mode.
/// </summary>
public sealed class GetPendingMessagesEndpoint(IListPendingMessagesQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<GetPendingMessagesRequest, IReadOnlyList<ListPendingMessagesDto>>
{
    public override void Configure()
    {
        Get("internal/identity/outbox/pending-messages");
        Policies(InternalApiKeyDefaults.PolicyName);
        Description(d => d.Produces<IReadOnlyList<ListPendingMessagesDto>>(200));
    }

    public override async Task HandleAsync(GetPendingMessagesRequest req, CancellationToken ct)
    {
        var query = new ListPendingMessagesQuery(req.BatchSize, TimeSpan.FromSeconds(req.LockDurationSeconds));

        var result = await invoker.InvokeAsync(handler, query, ct);
        await OkFromResultAsync(result, ct);
    }
}
