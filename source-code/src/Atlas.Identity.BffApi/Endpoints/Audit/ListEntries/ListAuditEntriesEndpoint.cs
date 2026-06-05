using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.BuildingBlocks.Audit.Queries;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Audit.ListEntries;
using Atlas.Identity.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Audit.ListEntries;

/// <summary>
/// Identity audit entries endpoint.
/// Invokes the Identity audit query handler through the standard handler pipeline.
/// </summary>
public sealed class ListAuditEntriesEndpoint(
    IIdentityListAuditEntriesQueryHandler handler,
    IHandlerInvoker invoker,
    AuditLabelLocalizer auditLabelLocalizer)
    : AtlasEndpoint<ListAuditEntriesRequest, IReadOnlyList<AuditEntryResponse>>
{
    public override void Configure()
    {
        Get("bff/identity/audit/entries");
        Policies($"permission:{ModulePermissions.Audit.Read}");
        Description(d => d.Produces<IReadOnlyList<AuditEntryResponse>>());
    }

    public override async Task HandleAsync(ListAuditEntriesRequest req, CancellationToken ct)
    {
        var query = new ListAuditEntriesQuery(
            EntityTypeId: req.EntityTypeId,
            From: req.From,
            To: req.To,
            Action: req.Action,
            EntityId: req.EntityId
        );

        var result   = await invoker.InvokeAsync(handler, query, ct);
        var response = result.Map(x => AuditEntryResponse.FromList(x, auditLabelLocalizer));
        await OkFromResultAsync(response, ct);
    }
}
