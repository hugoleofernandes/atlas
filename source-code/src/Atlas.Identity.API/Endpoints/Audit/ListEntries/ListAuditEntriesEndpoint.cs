using Atlas.BuildingBlocks.AuditTrail.Queries;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Audit.ListEntries;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.API.Endpoints.Audit.ListEntries;

/// <summary>
/// Identity audit entries endpoint.
/// Invokes the Identity audit query handler through the standard handler pipeline.
/// </summary>
public sealed class ListAuditEntriesEndpoint(IIdentityListAuditEntriesQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<ListAuditEntriesRequest, IReadOnlyList<AuditEntryDto>>
{
    public override void Configure()
    {
        Get("identity/audit/entries");
        Policies($"permission:{IdentityModulePermissions.Tenant.Audit.Read}");
        Description(d => d.Produces<IReadOnlyList<AuditEntryDto>>());
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
        await OkFromResultAsync(result, ct);
    }
}
