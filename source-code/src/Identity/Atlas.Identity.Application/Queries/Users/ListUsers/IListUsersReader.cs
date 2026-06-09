namespace Atlas.Identity.Application.Queries.Users.ListUsers;

public interface IListUsersReader
{
    Task<IReadOnlyList<UserDto>> ListAsync(Guid tenantId, bool? isActive, CancellationToken ct);
}
