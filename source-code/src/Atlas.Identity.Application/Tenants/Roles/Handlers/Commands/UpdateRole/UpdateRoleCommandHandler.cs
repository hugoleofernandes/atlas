using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.UpdateRole;

public sealed class UpdateRoleCommandHandler : IUpdateRoleCommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IRequestContext _requestContext;
    private readonly IPermissionPolicy _permissionPolicy;
    private readonly IIdentityUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public UpdateRoleCommandHandler(
        ITenantRepository tenantRepository,
        IRequestContext requestContext,
        IPermissionPolicy permissionPolicy,
        IIdentityUnitOfWork uow)
    {
        _tenantRepository = tenantRepository;
        _requestContext = requestContext;
        _permissionPolicy = permissionPolicy;
        _uow = uow;
    }

    public async Task<UpdateRoleOutput> ExecuteAsync(UpdateRoleCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId
            ?? throw new TenantContextNotResolvedException();

        var tenant = await _tenantRepository
            .GetByIdWithRolesAsync(tenantId, ct)
            ?? throw new TenantNotFoundException(_requestContext.TenantName ?? tenantId.ToString());

        tenant.UpdateRole(cmd.RoleId, cmd.Name, cmd.PermissionCodes, _permissionPolicy.All);

        var role = tenant.Roles.Single(r => r.Id == cmd.RoleId);
        var permissions = role.Permissions.Select(p => p.Code).ToList().AsReadOnly();

        return new UpdateRoleOutput(role.Id, role.Name, permissions);
    }
}
