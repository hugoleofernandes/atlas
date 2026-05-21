using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.Exceptions;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Commands.UpdateRole;

public sealed class UpdateRoleCommandHandler : IUpdateRoleCommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IRequestContext _requestContext;
    private readonly IIdentityUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public UpdateRoleCommandHandler(
        ITenantRepository tenantRepository,
        IRequestContext requestContext,
        IIdentityUnitOfWork uow)
    {
        _tenantRepository = tenantRepository;
        _requestContext = requestContext;
        _uow = uow;
    }

    public async Task<UpdateRoleOutput> ExecuteAsync(UpdateRoleCommand cmd, CancellationToken ct)
    {
        var tenantName = _requestContext.TenantName
            ?? throw new TenantContextNotResolvedException();

        var tenant = await _tenantRepository
            .GetByNameWithRolesAsync(tenantName, ct)
            ?? throw new TenantNotFoundException(tenantName);

        tenant.UpdateRole(cmd.RoleId, cmd.Name, cmd.PermissionCodes);

        var role = tenant.Roles.Single(r => r.Id == cmd.RoleId);
        var permissions = role.Permissions.Select(p => p.Code).ToList().AsReadOnly();

        return new UpdateRoleOutput(role.Id, role.Name, permissions);
    }
}
