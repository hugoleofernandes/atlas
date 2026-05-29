using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Invitations.Exceptions;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.Identity.Domain.Users;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Commands.ResolveTenantAccess;

public sealed class ResolveTenantAccessCommandHandler : IResolveTenantAccessCommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IIdentityUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public ResolveTenantAccessCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IInvitationRepository invitationRepository,
        IIdentityUnitOfWork uow)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _invitationRepository = invitationRepository;
        _uow = uow;
    }

    public async Task<ResolveTenantAccessOutput> ExecuteAsync(ResolveTenantAccessCommand cmd, CancellationToken ct)
    {
        // Loads tenant by name (OIDC token context — TenantId not yet known)
        var tenant = await _tenantRepository
            .GetByNameWithRolesAsync(cmd.TenantName.ToLowerInvariant(), ct)
            ?? throw new TenantNotFoundException(cmd.TenantName);

        var email = Email.Create(cmd.Email);
        var externalId = ExternalId.Create(cmd.ExternalOid);

        // --- Existing user path ---
        var existingUser = await _userRepository.FindActiveByEmailAsync(tenant.Id, email, ct);
        if (existingUser is not null)
        {
            existingUser.ResolveExistingAccess(externalId);

            var existingRole = tenant.Roles.Single(r => r.Id == existingUser.RoleId);
            var existingPermissions = existingRole.Permissions.Select(p => p.Code).ToList().AsReadOnly();

            return new ResolveTenantAccessOutput(
                tenant.Id,
                tenant.Name,
                existingUser.Id,
                existingRole.Id,
                existingRole.Name,
                existingPermissions);
        }

        // --- New user from invitation path ---
        var invitation = await _invitationRepository.FindByEmailAsync(tenant.Id, email, ct)
            ?? throw new InvitationNotFoundException(cmd.Email);

        invitation.Use(); // validates not used, not expired; emits InvitationUsedDomainEvent

        var role = tenant.Roles.Single(r => r.Id == invitation.RoleId);
        var user = User.CreateFromInvitation(invitation, externalId, role.Name);

        await _userRepository.AddAsync(user, ct);

        var permissions = role.Permissions.Select(p => p.Code).ToList().AsReadOnly();

        return new ResolveTenantAccessOutput(
            tenant.Id,
            tenant.Name,
            user.Id,
            role.Id,
            role.Name,
            permissions);
    }
}
