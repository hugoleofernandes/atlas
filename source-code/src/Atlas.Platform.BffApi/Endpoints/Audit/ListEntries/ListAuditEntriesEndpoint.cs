using Atlas.BuildingBlocks.AuditTrail.Labels;
using Atlas.BuildingBlocks.AuditTrail.Queries;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Platform.Application.Queries.Audit.ListEntries;
using Atlas.SharedDomain.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Platform.BffApi.Endpoints.Audit.ListEntries;

/// <summary>
/// Platform audit entries endpoint.
/// Invokes the Platform audit query handler through the standard handler pipeline.
/// </summary>
public sealed class ListAuditEntriesEndpoint(
    IPlatformListAuditEntriesQueryHandler handler,
    IHandlerInvoker invoker,
    AuditLabelLocalizer auditLabelLocalizer)
    : AtlasEndpoint<ListAuditEntriesRequest, IReadOnlyList<AuditEntryResponse>>
{
    public override void Configure()
    {
        Get("bff/platform/audit/entries");
        Policies($"permission:{PlatformModulePermissions.Audit.Read}");
        Description(d => d.Produces<IReadOnlyList<AuditEntryResponse>>());
    }

    public override async Task HandleAsync(ListAuditEntriesRequest req, CancellationToken ct)
    {
        var query = new ListAuditEntriesQuery(
            EntityTypeId: req.EntityTypeId,
            From:         req.From,
            To:           req.To,
            Action:       req.Action,
            EntityId:     req.EntityId);

        var result   = await invoker.InvokeAsync(handler, query, ct);
        var response = result.Map(x => AuditEntryResponse.FromList(x, auditLabelLocalizer));
        await OkFromResultAsync(response, ct);
    }
}
