using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Domain.Entities.Tenants.Events;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.Exceptions;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Commands.RemoveRole;

public sealed class RemoveRoleCommandHandler : IRemoveRoleCommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IRequestContext _requestContext;
    private readonly IIdentityUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public RemoveRoleCommandHandler(
        ITenantRepository tenantRepository,
        IRequestContext requestContext,
        IIdentityUnitOfWork uow)
    {
        _tenantRepository = tenantRepository;
        _requestContext = requestContext;
        _uow = uow;
    }

    public async Task<RemoveRoleOutput> ExecuteAsync(RemoveRoleCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId
            ?? throw new TenantContextNotResolvedException();

        var tenant = await _tenantRepository
            .GetByIdWithUsersAllInvitationsAndRolesAsync(tenantId, ct)
            ?? throw new TenantNotFoundException(_requestContext.TenantName ?? tenantId.ToString());

        tenant.RemoveRole(cmd.RoleId);

        var wasPhysicallyDeleted = tenant.DomainEvents
            .OfType<RoleDeletedDomainEvent>()
            .Any();

        return new RemoveRoleOutput(wasPhysicallyDeleted);
    }
}
