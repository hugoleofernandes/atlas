namespace Atlas.Identity.Application.Queries.Users.GetUserById;

public interface IGetUserByIdReader
{
    Task<GetUserByIdDto?> GetByIdAsync(Guid tenantId, Guid userId, CancellationToken ct);
}
