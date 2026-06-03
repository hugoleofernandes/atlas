using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Queries.Users.ListUsers;

public sealed class ListUsersQueryHandler(IListUsersReader reader, IRequestContext context) : IListUsersQueryHandler
{
    public Task<IReadOnlyList<UserDto>> ExecuteAsync(ListUsersQuery query, CancellationToken ct)
        => reader.ListAsync(context.TenantId!.Value, query.IsActive, ct);
}
