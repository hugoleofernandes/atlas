using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Invitations.Exceptions;
using Atlas.Identity.Domain.Shared;
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

    public IUnitOfWork UnitOfWork => _uow;

    public ResolveTenantAccessCommandHandler(
        IRoleRepository       roleRepository,
        IUserRepository       userRepository,
        IInvitationRepository invitationRepository,
        IIdentityUnitOfWork   uow,
        IRequestContextSetter contextSetter)
    {
        _roleRepository       = roleRepository;
        _userRepository       = userRepository;
        _invitationRepository = invitationRepository;
        _uow                  = uow;
        _contextSetter        = contextSetter;
    }

    public async Task<ResolveTenantAccessOutput> ExecuteAsync(ResolveTenantAccessCommand cmd, CancellationToken ct)
    {
        var tenantId   = cmd.TenantId;
        var tenantName = cmd.TenantName;
        var email      = Email.Create(cmd.Email);
        var externalId = ExternalId.Create(cmd.ExternalOid);

        // Suspend the global tenant query filter for the bootstrap queries.
        // At this point IRequestContext.TenantId is not yet populated.
        using (_contextSetter.SuspendTenantFilter())
        {
            // --- Existing user path ---
            var existingUser = await _userRepository.FindActiveByEmailAsync(tenantId, email, ct);
            if (existingUser is not null)
            {
                existingUser.ResolveExistingAccess(externalId);

                var existingRole = await _roleRepository.GetByIdWithPermissionsAsync(existingUser.RoleId, ct)
                    ?? throw new RoleNotFoundException(existingUser.RoleId);

                var existingPermissions = existingRole.Permissions.Select(p => p.Code).ToList().AsReadOnly();

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

            var user = User.CreateFromInvitation(invitation, externalId, role.Name);

            await _userRepository.AddAsync(user, ct);

            var permissions = role.Permissions.Select(p => p.Code).ToList().AsReadOnly();

            return new ResolveTenantAccessOutput(
                tenantId,
                tenantName,
                user.Id,
                role.Id,
                role.Name,
                permissions);
        }
    }
}
