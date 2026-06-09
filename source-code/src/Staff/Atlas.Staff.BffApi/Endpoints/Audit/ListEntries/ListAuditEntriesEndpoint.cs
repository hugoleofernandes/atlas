using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.BuildingBlocks.Audit.Queries;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Application.StaffMembers.Queries.Audit.ListEntries;
using Atlas.Staff.Contracts.Permissions;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Staff.BffApi.Endpoints.Audit.ListEntries;

/// <summary>
/// Staff audit entries endpoint.
/// Invokes the Staff audit query handler through the standard handler pipeline.
/// </summary>
public sealed class ListAuditEntriesEndpoint(
    IStaffListAuditEntriesQueryHandler handler,
    IHandlerInvoker invoker,
    AuditLabelLocalizer auditLabelLocalizer
) : AtlasEndpoint<ListAuditEntriesRequest, IReadOnlyList<AuditEntryResponse>>
{
    public override void Configure()
    {
        Get("bff/v1/staff/audit/entries");
        Policies($"permission:{StaffModulePermissions.Audit.Read}");
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

        var result = await invoker.InvokeAsync(handler, query, ct);
        var response = result.Map(x => AuditEntryResponse.FromList(x, auditLabelLocalizer));
        await OkFromResultAsync(response, ct);
    }
}
