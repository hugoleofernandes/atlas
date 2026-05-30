using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.Identity.Domain.Users.Exceptions;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.Hosting;

namespace Atlas.Identity.Application.Commands.DevLogin;

/// <summary>
/// Development-only command handler — resolves tenant access without going through OIDC.
///
/// Existing user path: pulls the stored ExternalId from the database and passes it to
/// ResolveExistingAccess, so the identity check always passes regardless of how the user
/// was originally created (real Entra login or a previous dev login).
///
/// New user path: generates a deterministic fake OID from the email and creates the user
/// from a pending invitation. Subsequent dev logins will use the stored OID (existing user path).
///
/// Throws InvalidOperationException outside of Development — the endpoint also guards this,
/// but the handler is the last line of defense.
/// </summary>
public sealed class DevLoginCommandHandler : IDevLoginCommandHandler
{
    private readonly ITenantRepository     _tenantRepository;
    private readonly IUserRepository       _userRepository;
    private readonly IRequestContextSetter _contextSetter;
    private readonly IHostEnvironment      _env;

    public IUnitOfWork UnitOfWork => throw new NotSupportedException("DevLoginCommandHandler does not write to the database.");

    public DevLoginCommandHandler(
        ITenantRepository     tenantRepository,
        IUserRepository       userRepository,
        IRequestContextSetter contextSetter,
        IHostEnvironment      env)
    {
        _tenantRepository = tenantRepository;
        _userRepository   = userRepository;
        _contextSetter    = contextSetter;
        _env              = env;
    }

    public async Task<DevLoginOutput> ExecuteAsync(DevLoginCommand cmd, CancellationToken ct)
    {
        if (!_env.IsDevelopment())
            throw new InvalidOperationException(
                "DevLoginCommandHandler must not be called outside of the Development environment.");

        var tenant = await _tenantRepository.GetByNameWithRolesAsync(cmd.TenantName.ToLowerInvariant(), ct)
            ?? throw new TenantNotFoundException(cmd.TenantName);

        var email = Email.Create(cmd.Email);

        using (_contextSetter.SuspendTenantFilter())
        {
            var user = await _userRepository.FindActiveByEmailAsync(tenant.Id, email, ct)
                ?? throw new UserNotFoundException(cmd.Email);

            user.ResolveExistingAccess(user.ExternalId);

            var role        = tenant.Roles.Single(r => r.Id == user.RoleId);
            var permissions = role.Permissions.Select(p => p.Code).ToList().AsReadOnly();

            return new DevLoginOutput(
                TenantId:    tenant.Id,
                TenantName:  tenant.Name,
                UserId:      user.Id,
                RoleId:      role.Id,
                RoleName:    role.Name,
                Permissions: permissions);
        }
    }
}
