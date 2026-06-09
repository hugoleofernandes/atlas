using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Tenants._Roles.Exceptions;
using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Commands.RemoveRole;

public sealed class RemoveRoleCommandHandler : IRemoveRoleCommandHandler
{
    private readonly IRoleRepository       _roleRepository;
    private readonly IUserRepository       _userRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IRequestContext       _requestContext;
    private readonly IIdentityUnitOfWork   _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public RemoveRoleCommandHandler(
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

    public async Task<RemoveRoleOutput> ExecuteAsync(RemoveRoleCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId
            ?? throw new TenantContextNotResolvedException();

        var role = await _roleRepository.GetByIdWithPermissionsAsync(cmd.RoleId, ct)
            ?? throw new RoleNotFoundException(cmd.RoleId);

        if (role.IsSystem)
            throw new SystemRoleCannotBeModifiedException(role.Name);

        // Run all four usage queries in parallel — avoids 4 sequential round trips
        var hasActiveUsersTask       = _userRepository.HasActiveWithRoleAsync(tenantId, cmd.RoleId, ct);
        var hasActiveInvitationsTask = _invitationRepository.HasActiveWithRoleAsync(tenantId, cmd.RoleId, ct);
        var hasAnyUsersTask          = _userRepository.HasAnyWithRoleAsync(tenantId, cmd.RoleId, ct);
        var hasAnyInvitationsTask    = _invitationRepository.HasAnyWithRoleAsync(tenantId, cmd.RoleId, ct);

        await Task.WhenAll(hasActiveUsersTask, hasActiveInvitationsTask, hasAnyUsersTask, hasAnyInvitationsTask);

        if (hasActiveUsersTask.Result)
            throw new RoleInUseByUsersException(role.Name);

        if (hasActiveInvitationsTask.Result)
            throw new RoleInUseByInvitationsException(role.Name);

        var hasHistory = hasAnyUsersTask.Result || hasAnyInvitationsTask.Result;

        if (hasHistory)
        {
            // Soft delete — preserve history
            role.Deactivate();
        }
        else
        {
            // Hard delete — no references exist
            role.Delete();
            _roleRepository.Remove(role);
        }

        return new RemoveRoleOutput(WasPhysicallyDeleted: !hasHistory);
    }
}
