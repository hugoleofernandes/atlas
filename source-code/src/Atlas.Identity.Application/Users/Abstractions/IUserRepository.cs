using Atlas.Identity.Domain.Users;

namespace Atlas.Identity.Application.Users.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByExternalIdAsync(
        string externalId,
        CancellationToken ct);

    Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken ct);

    Task AddAsync(
        User user,
        CancellationToken ct);
}