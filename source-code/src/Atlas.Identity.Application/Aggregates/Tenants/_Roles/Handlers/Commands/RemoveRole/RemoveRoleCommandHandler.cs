using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Aggregates.Invitations;
using Atlas.Identity.Application.Aggregates.Tenants;
using Atlas.Identity.Application.Aggregates.Users;
using Atlas.Identity.Domain.Tenants.Events;
using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Commands.RemoveRole;

public sealed class RemoveRoleCommandHandler : IRemoveRoleCommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IRequestContext _requestContext;
    private readonly IIdentityUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public RemoveRoleCommandHandler(
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

    public async Task<RemoveRoleOutput> ExecuteAsync(RemoveRoleCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId
            ?? throw new TenantContextNotResolvedException();

        var tenant = await _tenantRepository
            .GetByIdWithRolesAsync(tenantId, ct)
            ?? throw new TenantNotFoundException(_requestContext.TenantName ?? tenantId.ToString());

        // Run all four role-usage queries in parallel — avoids 4 sequential round trips
        var hasActiveUsersTask       = _userRepository.HasActiveWithRoleAsync(tenantId, cmd.RoleId, ct);
        var hasActiveInvitationsTask = _invitationRepository.HasActiveWithRoleAsync(tenantId, cmd.RoleId, ct);
        var hasAnyUsersTask          = _userRepository.HasAnyWithRoleAsync(tenantId, cmd.RoleId, ct);
        var hasAnyInvitationsTask    = _invitationRepository.HasAnyWithRoleAsync(tenantId, cmd.RoleId, ct);

        await Task.WhenAll(hasActiveUsersTask, hasActiveInvitationsTask, hasAnyUsersTask, hasAnyInvitationsTask);

        var hasHistory = hasAnyUsersTask.Result || hasAnyInvitationsTask.Result;

        tenant.RemoveRole(
            cmd.RoleId,
            hasActiveUsersTask.Result,
            hasActiveInvitationsTask.Result,
            hasHistory);

        var wasPhysicallyDeleted = tenant.DomainEvents
            .OfType<RoleDeletedDomainEvent>()
            .Any();

        return new RemoveRoleOutput(wasPhysicallyDeleted);
    }
}
