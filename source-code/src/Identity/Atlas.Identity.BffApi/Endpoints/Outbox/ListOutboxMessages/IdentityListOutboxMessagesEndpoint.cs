using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Contracts.Permissions;
using Atlas.Outbox.Application.Queries.ListOutboxMessages;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Outbox.ListOutboxMessages;

public sealed class IdentityListOutboxMessagesEndpoint(
    IIdentityListOutboxMessagesQueryHandler handler,
    IHandlerInvoker invoker)
    : AtlasEndpoint<ListOutboxMessagesRequest, IReadOnlyList<OutboxMessageRow>>
{
    public override void Configure()
    {
        Get("bff/v1/outbox/identity/messages");
        Policies($"permission:{IdentityModulePermissions.Outbox.Read}");
        Description(d => d.Produces<IReadOnlyList<OutboxMessageRow>>());
    }

    public override async Task HandleAsync(ListOutboxMessagesRequest req, CancellationToken ct)
    {
        var query = new ListOutboxMessagesQuery(req.From, req.To);
        var result = await invoker.InvokeAsync(handler, query, ct);
        await OkFromResultAsync(result, ct);
    }
}
