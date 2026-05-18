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
            .GetByNameWithUsersInvitationsAndRolesAsync(
                cmd.TenantName.ToLowerInvariant(), ct)
            ?? throw new TenantNotFoundException(cmd.TenantName);

        // If the user already exists, use their stored ExternalId.
        // This makes the command idempotent: the caller's OID is only meaningful
        // on first access (when the user is created from the invitation).
        // In production the Entra OID never changes, so this is a no-op.
        // In dev login the fake OID may differ from a previously stored OID.
        var email = Email.Create(cmd.Email);
        var existingExternalId = tenant.Users
            .FirstOrDefault(u => u.Email.Value == email.Value && u.IsActive)
            ?.ExternalId.Value;

        var user = tenant.ResolveAccess(
            ExternalId.Create(existingExternalId ?? cmd.ExternalOid),
            email);

        var role = tenant.Roles.Single(r => r.Id == user.RoleId);
        var permissions = role.Permissions.Select(p => p.Code).ToList().AsReadOnly();

        return new Output(
            tenant.Id,
            tenant.Name,
            user.Id,
            role.Id,
            role.Name,
            permissions);
    }
}
