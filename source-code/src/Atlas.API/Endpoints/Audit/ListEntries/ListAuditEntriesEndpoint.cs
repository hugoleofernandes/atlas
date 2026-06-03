using Atlas.BuildingBlocks.AuditTrail.Labels;
using Atlas.BuildingBlocks.AuditTrail.Queries;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Audit.ListEntries;
using Atlas.Identity.Contracts;
using Atlas.Identity.Contracts.Permissions;
using Atlas.Platform.Application.Queries.Audit.ListEntries;
using Atlas.SharedKernel.Domain.Permissions;
using StaffPermissions = Atlas.Staff.Contracts.Permissions;
using PlatformPermissions = Atlas.Platform.Contracts.Permissions;
using PlatformContracts = Atlas.Platform.Contracts;
using StaffContracts = Atlas.Staff.Contracts;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Application.StaffMembers.Queries.Audit.ListEntries;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

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
            entityTypeId == EntityTypes.UserId
            || entityTypeId == EntityTypes.RoleId
            || entityTypeId == EntityTypes.InvitationId
        )
        {
            return new AuditTarget(
                ModulePermissions.Audit.Read,
                (query, ct) => invoker.InvokeAsync(identityHandler, query, ct)
            );
        }

        if (entityTypeId == StaffContracts.EntityTypes.StaffMemberId)
        {
            return new AuditTarget(
                StaffPermissions.ModulePermissions.Audit.Read,
                (query, ct) => invoker.InvokeAsync(staffHandler, query, ct)
            );
        }

        if (entityTypeId == PlatformContracts.EntityTypes.TenantId)
        {
            return new AuditTarget(
                PlatformPermissions.ModulePermissions.Audit.Read,
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
