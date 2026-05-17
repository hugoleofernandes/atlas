using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Domain.Exceptions;
using Atlas.Identity.Domain.ValueObjects;

namespace Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

public sealed class CommandHandler : ICommandHandler
{
    private readonly ITenantRepository _tenantRepository;

    public CommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Output> ExecuteAsync(Command cmd, CancellationToken ct)
    {
        var tenant = await _tenantRepository
            .GetByNameWithUsersAndInvitationsAsync(
                cmd.TenantName.ToLowerInvariant(), ct)
            ?? throw new TenantNotFoundException(cmd.TenantName);

        var user = tenant.ResolveAccess(
            ExternalId.Create(cmd.ExternalOid),
            Email.Create(cmd.Email));

        return new Output(
            tenant.Id,
            tenant.Name,
            user.Id,
            user.Role.Value);
    }
}
