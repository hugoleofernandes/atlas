using Atlas.BuildingBlocks.AuditTrail.FastEndpoints.ListEntries;
using Atlas.Identity.Application.Queries.Audit.ListEntries;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.API.Endpoints.Audit.ListEntries;

/// <summary>
/// Identity audit entries endpoint.
/// The request handling is implemented by AuditEntriesEndpointBase:
/// it maps query params to ListAuditEntriesQuery and invokes
/// IIdentityListAuditEntriesQueryHandler through IHandlerInvoker.
/// </summary>
public sealed class ListAuditEntriesEndpoint(IIdentityListAuditEntriesQueryHandler handler, IHandlerInvoker invoker)
    : AuditEntriesEndpointBase<IIdentityListAuditEntriesQueryHandler>(handler, invoker)
{
    protected override string Route => "identity/audit/entries";

    protected override string Permission => IdentityModulePermissions.Tenant.Audit.Read;
}
