using Atlas.BuildingBlocks.AuditTrail.Queries;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.BuildingBlocks.AuditTrail.FastEndpoints.ListEntries;

/// <summary>
/// Base endpoint for module-owned audit trail reads.
/// Each module provides only its route, permission, and module-specific query handler.
/// </summary>
public abstract class AuditEntriesEndpointBase<THandler>(
    THandler handler,
    IHandlerInvoker invoker)
    : AtlasEndpoint<ListAuditEntriesRequest, IReadOnlyList<AuditEntryDto>>
    where THandler : IQueryHandler<ListAuditEntriesQuery, IReadOnlyList<AuditEntryDto>>
{
    protected abstract string Route { get; }
    protected abstract string Permission { get; }

    public override void Configure()
    {
        Get(Route);
        Policies($"permission:{Permission}");
        Description(d => d.Produces<IReadOnlyList<AuditEntryDto>>());
    }

    public override async Task HandleAsync(ListAuditEntriesRequest req, CancellationToken ct)
    {
        var query = new ListAuditEntriesQuery(
            EntityTypeId: req.EntityTypeId,
            From:         req.From,
            To:           req.To,
            Action:       req.Action,
            EntityId:     req.EntityId);

        var result = await invoker.InvokeAsync(handler, query, ct);
        await OkFromResultAsync(result, ct);
    }
}
