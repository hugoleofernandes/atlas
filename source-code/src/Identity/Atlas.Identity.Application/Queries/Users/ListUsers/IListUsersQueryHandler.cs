using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Queries.Users.ListUsers;

public interface IListUsersQueryHandler : IQueryHandler<ListUsersQuery, IReadOnlyList<UserDto>>;
