using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.BuildingBlocks.Audit.Queries;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Audit.ListEntries;
using Atlas.Identity.Domain.ModulePermissions;
using Atlas.Platform.Application.Queries.Audit.ListEntries;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.EntityTypes;
using Atlas.Staff.Application.StaffMembers.Queries.Audit.ListEntries;
using Atlas.Staff.Domain.ModulePermissions;
using Microsoft.AspNetCore.Authorization;

namespace Atlas.API.Endpoints.Audit.ListEntries;

/// <summary>
/// Aggregates module-owned audit endpoints for frontend convenience.
/// The module remains the owner of its query handler; this endpoint only routes by EntityTypeId.
/// </summary>
public sealed class ListAuditEntriesEndpoint(
    IIdentityListAuditEntriesQueryHandler identityHandler,
    IStaffListAuditEntriesQueryHandler staffHandler,
    IPlatformListAuditEntriesQueryHandler platformHandler,
    IHandlerInvoker invoker,
    IAuthorizationService authorizationService,
    AuditLabelLocalizer auditLabelLocalizer
) : AtlasEndpoint<ListAuditEntriesRequest, IReadOnlyList<AuditEntryResponse>>
{
    public override void Configure()
    {
        Get("bff/audit/entries");
        Description(d => d.Produces<IReadOnlyList<AuditEntryResponse>>());
    }

    public override async Task HandleAsync(ListAuditEntriesRequest req, CancellationToken ct)
    {
        var target = ResolveTarget(req.EntityTypeId);
        if (target is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var authorization = await authorizationService.AuthorizeAsync(
            User,
            policyName: $"permission:{target.Permission}"
        );

        if (!authorization.Succeeded)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        var query = new ListAuditEntriesQuery(
            EntityTypeId: req.EntityTypeId,
            From: req.From,
            To: req.To,
            Action: req.Action,
            EntityId: req.EntityId
        );

        var result = await target.ExecuteAsync(query, ct);
        var response = result.Map(x => AuditEntryResponse.FromList(x, auditLabelLocalizer));
        await OkFromResultAsync(response, ct);
    }

    private AuditTarget? ResolveTarget(Guid entityTypeId)
    {
        if (
            entityTypeId == IdentityEntityTypes.User.Id
            || entityTypeId == IdentityEntityTypes.Role.Id
            || entityTypeId == IdentityEntityTypes.Invitation.Id
        )
        {
            return new AuditTarget(
                IdentityModulePermissions.Audit.Read,
                (query, ct) => invoker.InvokeAsync(identityHandler, query, ct)
            );
        }

        if (entityTypeId == StaffEntityTypes.StaffMember.Id)
        {
            return new AuditTarget(
                StaffModulePermissions.Audit.Read,
                (query, ct) => invoker.InvokeAsync(staffHandler, query, ct)
            );
        }

        if (entityTypeId == PlatformEntityTypes.Tenant.Id)
        {
            return new AuditTarget(
                StaffModulePermissions.Audit.Read,
                (query, ct) => invoker.InvokeAsync(platformHandler, query, ct)
            );
        }

        return null;
    }

    private sealed record AuditTarget(
        string Permission,
        Func<ListAuditEntriesQuery, CancellationToken, Task<Result<IReadOnlyList<AuditEntryDto>>>> ExecuteAsync
    );
}
