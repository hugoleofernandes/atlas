using Atlas.BuildingBlocks.AuditTrail.Queries;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Application.StaffMembers.Queries.Audit.ListEntries;
using Atlas.Staff.Domain.Permissions;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Staff.API.Endpoints.Audit.ListEntries;

/// <summary>
/// Staff audit entries endpoint.
/// Invokes the Staff audit query handler through the standard handler pipeline.
/// </summary>
public sealed class ListAuditEntriesEndpoint(IStaffListAuditEntriesQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<ListAuditEntriesRequest, IReadOnlyList<AuditEntryDto>>
{
    public override void Configure()
    {
        Get("staff/audit/entries");
        Policies($"permission:{StaffPermissions.Audit.Read}");
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
