using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Queries.Users.GetUserById;

public sealed class GetUserByIdQueryHandler(IGetUserByIdReader reader, IRequestContext context) : IGetUserByIdQueryHandler
{
    public Task<GetUserByIdDto?> ExecuteAsync(GetUserByIdQuery query, CancellationToken ct)
        => reader.GetByIdAsync(context.TenantId!.Value, query.UserId, ct);
}
