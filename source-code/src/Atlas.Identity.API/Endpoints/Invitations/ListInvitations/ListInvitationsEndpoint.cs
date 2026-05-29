using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Invitations.ListInvitations;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.API.Endpoints.Invitations.ListInvitations;

/// <summary>
/// Lists invitations for the authenticated user's tenant, ordered by most recent first.
/// </summary>
public sealed class ListInvitationsEndpoint(
    IListInvitationsQueryHandler handler,
    IHandlerInvoker invoker
) : AtlasEndpoint<ListInvitationsRequest, IReadOnlyList<InvitationDto>>
{
    public override void Configure()
    {
        Get("identity/invitations");
        Policies($"permission:{IdentityModulePermissions.Tenant.Invitations.Read}");
        Description(d => d.Produces<IReadOnlyList<InvitationDto>>());
    }

    public override async Task HandleAsync(ListInvitationsRequest req, CancellationToken ct)
    {
        var query  = new ListInvitationsQuery(req.IsActive);
        var result = await invoker.InvokeAsync(handler, query, ct);
        await OkFromResultAsync(result, ct);
    }
}
