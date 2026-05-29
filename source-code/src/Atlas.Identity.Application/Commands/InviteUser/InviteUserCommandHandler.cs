using Atlas.Identity.Application.Abstractions;
using Atlas.SharedKernel.Application;
using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Invitations.Exceptions;
using Atlas.Identity.Domain.Users.Exceptions;
using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Application.Repositories;

namespace Atlas.Identity.Application.Commands.InviteUser;

public sealed class InviteUserCommandHandler : IInviteUserCommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IRequestContext _requestContext;
    private readonly IIdentityUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public InviteUserCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IInvitationRepository invitationRepository,
        IRequestContext requestContext,
        IIdentityUnitOfWork uow)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _invitationRepository = invitationRepository;
        _requestContext = requestContext;
        _uow = uow;
    }

    public async Task<InviteUserOutput> ExecuteAsync(InviteUserCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId
            ?? throw new TenantContextNotResolvedException();

        var email = Email.Create(cmd.Email);

        // Pre-checks — avoid loading the tenant aggregate for invariants it no longer owns
        if (await _userRepository.ExistsWithEmailAsync(tenantId, email, ct))
            throw new UserAlreadyExistsException(cmd.Email);

        if (await _invitationRepository.HasActiveForEmailAsync(tenantId, email, ct))
            throw new DuplicateInvitationException(cmd.Email);

        var tenant = await _tenantRepository
            .GetByIdWithRolesAsync(tenantId, ct)
            ?? throw new TenantNotFoundException(_requestContext.TenantName ?? tenantId.ToString());

        // Tenant validates: active + role exists
        tenant.EnsureRoleExists(cmd.RoleId);

        var invitation = Invitation.Create(tenant.Id, email, cmd.RoleId, InvitationTtl.Default);

        await _invitationRepository.AddAsync(invitation, ct);

        var role = tenant.Roles.Single(r => r.Id == cmd.RoleId);

        return new InviteUserOutput(
            invitation.Id,
            invitation.Email.Value,
            role.Id,
            role.Name,
            invitation.ExpiresAt);
    }
}
