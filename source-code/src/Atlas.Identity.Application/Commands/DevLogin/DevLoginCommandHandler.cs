using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Tenants._Roles.Exceptions;
using Atlas.Identity.Domain.Users;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.Hosting;

namespace Atlas.Identity.Application.Commands.DevLogin;

/// <summary>
/// Development-only handler — resolves tenant access without going through OIDC.
/// No writes, routes through CommandHandlerInvoker with NullUnitOfWork.
///
/// Receives TenantId + TenantName already resolved by Platform's GetTenantByNameQueryHandler.
///
/// If no user exists for the given email, creates a fake in-memory user (not persisted)
/// assigned to the first active role of the tenant.
///
/// Throws InvalidOperationException outside of Development — the endpoint also guards this,
/// but the handler is the last line of defense.
/// </summary>
public sealed class DevLoginCommandHandler : IDevLoginCommandHandler
{
    private readonly IRoleRepository         _roleRepository;
    private readonly IUserRepository         _userRepository;
    private readonly IRequestContextSetter   _contextSetter;
    private readonly IPermissionCatalogCache _cache;
    private readonly IHostEnvironment        _env;

    public DevLoginCommandHandler(
        IRoleRepository         roleRepository,
        IUserRepository         userRepository,
        IRequestContextSetter   contextSetter,
        IPermissionCatalogCache cache,
        IHostEnvironment        env)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _contextSetter  = contextSetter;
        _cache          = cache;
        _env            = env;
    }

    public async Task<DevLoginOutput> ExecuteAsync(DevLoginCommand cmd, CancellationToken ct)
    {
        if (!_env.IsDevelopment())
            throw new InvalidOperationException(
                "DevLoginCommandHandler must not be called outside of the Development environment.");

        var email = Email.Create(cmd.Email);

        using (_contextSetter.SuspendTenantFilter())
        {
            var roles = await _roleRepository.GetByTenantIdWithPermissionsAsync(cmd.TenantId, ct);

            var user = await _userRepository.FindActiveByEmailAsync(cmd.TenantId, email, ct);
            if (user is null)
            {
                var firstActiveRole = roles.First(r => r.IsActive);
                user = User.CreateForDev(cmd.TenantId, email, firstActiveRole.Id);
            }

            user.ResolveExistingAccess(user.ExternalId);

            var role = roles.Single(r => r.Id == user.RoleId);
            if (!role.IsActive)
                throw new RoleInactiveException(role.Name);

            var permissionIds = role.Permissions.Select(p => p.PermissionId).ToHashSet();
            var all = await _cache.GetAllActiveAsync(ct);
            var permissions = all
                .Where(p => permissionIds.Contains(p.Id))
                .Select(p => p.Code)
                .ToList()
                .AsReadOnly();

            return new DevLoginOutput(
                TenantId:    cmd.TenantId,
                TenantName:  cmd.TenantName,
                UserId:      user.Id,
                RoleId:      role.Id,
                RoleName:    role.Name,
                Permissions: permissions);
        }
    }
}
