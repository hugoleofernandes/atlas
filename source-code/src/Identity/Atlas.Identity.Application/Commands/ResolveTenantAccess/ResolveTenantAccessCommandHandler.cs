using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Invitations.Exceptions;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Tenants._Roles;
using Atlas.Identity.Domain.Tenants._Roles.Exceptions;
using Atlas.Identity.Domain.Users;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Commands.ResolveTenantAccess;

public sealed class ResolveTenantAccessCommandHandler : IResolveTenantAccessCommandHandler
{
    private readonly IRoleRepository       _roleRepository;
    private readonly IUserRepository       _userRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IIdentityUnitOfWork   _uow;
    private readonly IRequestContextSetter _contextSetter;
    private readonly IPermissionCatalogCache _cache;

    public IUnitOfWork UnitOfWork => _uow;

    public ResolveTenantAccessCommandHandler(
        IRoleRepository       roleRepository,
        IUserRepository       userRepository,
        IInvitationRepository invitationRepository,
        IIdentityUnitOfWork   uow,
        IRequestContextSetter contextSetter,
        IPermissionCatalogCache cache)
    {
        _roleRepository       = roleRepository;
        _userRepository       = userRepository;
        _invitationRepository = invitationRepository;
        _uow                  = uow;
        _contextSetter        = contextSetter;
        _cache                = cache;
    }

    public async Task<ResolveTenantAccessOutput> ExecuteAsync(ResolveTenantAccessCommand cmd, CancellationToken ct)
    {
        var tenantId   = cmd.TenantId;
        var tenantName = cmd.TenantName;
        var email      = Email.Create(cmd.Email);
        var externalId = ExternalId.Create(cmd.ExternalOid);

        using (_contextSetter.SuspendTenantFilter())
        {
            // --- Existing user path ---
            var existingUser = await _userRepository.FindActiveByEmailAsync(tenantId, email, ct);
            if (existingUser is not null)
            {
                existingUser.ResolveExistingAccess(externalId);
                _contextSetter.Set(tenantId, tenantName, existingUser.Id, email.Value);

                var existingRole = await _roleRepository.GetByIdWithPermissionsAsync(existingUser.RoleId, ct)
                    ?? throw new RoleNotFoundException(existingUser.RoleId);

                if (!existingRole.IsActive)
                    throw new RoleInactiveException(existingRole.Name);

                var existingPermissions = await ResolveCodesAsync(existingRole, ct);

                return new ResolveTenantAccessOutput(
                    tenantId,
                    tenantName,
                    existingUser.Id,
                    existingRole.Id,
                    existingRole.Name,
                    existingPermissions);
            }

            // --- New user from invitation path ---
            var invitation = await _invitationRepository.FindByEmailAsync(tenantId, email, ct)
                ?? throw new InvitationNotFoundException(cmd.Email);

            invitation.Use();

            var role = await _roleRepository.GetByIdWithPermissionsAsync(invitation.RoleId, ct)
                ?? throw new RoleNotFoundException(invitation.RoleId);

            if (!role.IsActive)
                throw new RoleInactiveException(role.Name);

            var user = User.CreateFromInvitation(invitation, externalId, role.Name);
            _contextSetter.Set(tenantId, tenantName, user.Id, email.Value);

            await _userRepository.AddAsync(user, ct);

            var permissions = await ResolveCodesAsync(role, ct);

            return new ResolveTenantAccessOutput(
                tenantId,
                tenantName,
                user.Id,
                role.Id,
                role.Name,
                permissions);
        }
    }

    private async Task<IReadOnlyList<string>> ResolveCodesAsync(Role role, CancellationToken ct)
    {
        if (role.Permissions.Count == 0)
            return [];

        var permissionIds = role.Permissions.Select(p => p.PermissionId).ToHashSet();
        var all = await _cache.GetAllActiveAsync(ct);
        return all
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Code)
            .ToList()
            .AsReadOnly();
    }
}
