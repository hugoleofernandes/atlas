using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Invitations.Exceptions;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Tenants._Roles.Exceptions;
using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.Identity.Domain.Users.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Commands.InviteUser;

public sealed class InviteUserCommandHandler : IInviteUserCommandHandler
{
    private readonly IRoleRepository       _roleRepository;
    private readonly IUserRepository       _userRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IRequestContext       _requestContext;
    private readonly IIdentityUnitOfWork   _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public InviteUserCommandHandler(
        IRoleRepository       roleRepository,
        IUserRepository       userRepository,
        IInvitationRepository invitationRepository,
        IRequestContext       requestContext,
        IIdentityUnitOfWork   uow)
    {
        _roleRepository       = roleRepository;
        _userRepository       = userRepository;
        _invitationRepository = invitationRepository;
        _requestContext       = requestContext;
        _uow                  = uow;
    }

    public async Task<InviteUserOutput> ExecuteAsync(InviteUserCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId
            ?? throw new TenantContextNotResolvedException();

        var email = Email.Create(cmd.Email);

        // Pre-checks — run before loading any aggregate
        if (await _userRepository.ExistsWithEmailAsync(tenantId, email, ct))
            throw new UserAlreadyExistsException(cmd.Email);

        if (await _invitationRepository.HasActiveForEmailAsync(tenantId, email, ct))
            throw new DuplicateInvitationException(cmd.Email);

        // Security: verify role belongs to this tenant and is active
        var role = await _roleRepository.GetByIdWithPermissionsAsync(cmd.RoleId, ct);

        if (role is null || role.TenantId != tenantId || !role.IsActive)
            throw new RoleNotFoundException(cmd.RoleId);

        var invitation = Invitation.Create(tenantId, email, cmd.RoleId, InvitationTtl.Default);

        await _invitationRepository.AddAsync(invitation, ct);

        return new InviteUserOutput(
            invitation.Id,
            invitation.Email.Value,
            role.Id,
            role.Name,
            invitation.ExpiresAt);
    }
}
