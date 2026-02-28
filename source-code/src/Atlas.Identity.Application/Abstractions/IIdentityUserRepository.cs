using Atlas.Identity.Domain.Entities;

namespace Atlas.Identity.Application.Abstractions;

public interface IIdentityUserRepository
{
    Task<IdentityUser?> GetByExternalIdAsync(
        string externalId,
        CancellationToken ct);

    Task<IdentityUser?> GetByIdAsync(
        Guid id,
        CancellationToken ct);

    Task AddAsync(
        IdentityUser user,
        CancellationToken ct);
}