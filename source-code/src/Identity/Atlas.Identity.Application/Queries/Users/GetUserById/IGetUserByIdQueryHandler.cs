using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Queries.Users.GetUserById;

public interface IGetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, GetUserByIdDto?>;
