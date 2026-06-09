using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Invitations.ListInvitations;
using Atlas.Identity.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Invitations.ListInvitations;

/// <summary>
/// Lists invitations for the authenticated user's tenant, ordered by most recent first.
/// </summary>
public sealed class ListInvitationsEndpoint(IListInvitationsQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<ListInvitationsRequest, IReadOnlyList<InvitationDto>>
{
    public override void Configure()
    {
        Get("bff/v1/identity/invitations");
        Policies($"permission:{IdentityModulePermissions.Invitations.Read}");
        Description(d => d.Produces<IReadOnlyList<InvitationDto>>());
    }

    public override async Task HandleAsync(ListInvitationsRequest req, CancellationToken ct)
    {
        var query = new ListInvitationsQuery(req.IsActive);
        var result = await invoker.InvokeAsync(handler, query, ct);
        await OkFromResultAsync(result, ct);
    }
}
