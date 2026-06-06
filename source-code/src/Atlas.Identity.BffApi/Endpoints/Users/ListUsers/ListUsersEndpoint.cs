using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Users.ListUsers;
using Atlas.Identity.Domain.ModulePermissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Users.ListUsers;

/// <summary>
/// Returns users for the authenticated tenant, optionally filtered by active status.
/// </summary>
public sealed class ListUsersEndpoint(IListUsersQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<ListUsersRequest, IReadOnlyList<UserDto>>
{
    public override void Configure()
    {
        Get("bff/identity/users");
        Policies($"permission:{IdentityModulePermissions.Users.Read}");
        Description(d => d.Produces<IReadOnlyList<UserDto>>());
    }

    public override async Task HandleAsync(ListUsersRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new ListUsersQuery(req.IsActive), ct);
        await OkFromResultAsync(result, ct);
    }
}
