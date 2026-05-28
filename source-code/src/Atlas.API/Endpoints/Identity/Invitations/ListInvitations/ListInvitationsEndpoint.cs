using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Invitations.Handlers.Queries.ListInvitations;
using Atlas.Identity.Domain.Tenants.Roles.Permissions;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.API.Endpoints.Identity.Invitations.ListInvitations;

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
        Get("invitations");
        Policies($"permission:{IdentityPermissions.Tenant.Invitations.Read}");
        Description(d => d.Produces<IReadOnlyList<InvitationDto>>());
    }

    public override async Task HandleAsync(ListInvitationsRequest req, CancellationToken ct)
    {
        var query  = new ListInvitationsQuery(req.IsActive);
        var result = await invoker.InvokeAsync(handler, query, ct);
        await OkFromResultAsync(result, ct);
    }
}
