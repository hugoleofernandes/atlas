using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Tenants._Roles.Exceptions;
using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.Identity.Application.Commands.UpdateRole;

public sealed class UpdateRoleCommandHandler : IUpdateRoleCommandHandler
{
    private readonly IRoleRepository     _roleRepository;
    private readonly IRequestContext     _requestContext;
    private readonly IPermissionPolicy   _permissionPolicy;
    private readonly IIdentityUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public UpdateRoleCommandHandler(
        IRoleRepository     roleRepository,
        IRequestContext     requestContext,
        IPermissionPolicy   permissionPolicy,
        IIdentityUnitOfWork uow)
    {
        _roleRepository   = roleRepository;
        _requestContext   = requestContext;
        _permissionPolicy = permissionPolicy;
        _uow              = uow;
    }

    public async Task<UpdateRoleOutput> ExecuteAsync(UpdateRoleCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId
            ?? throw new TenantContextNotResolvedException();

        var role = await _roleRepository.GetByIdWithPermissionsAsync(cmd.RoleId, ct)
            ?? throw new RoleNotFoundException(cmd.RoleId);

        if (await _roleRepository.ExistsWithNameExcludingAsync(tenantId, cmd.Name, cmd.RoleId, ct))
            throw new RoleAlreadyExistsException(cmd.Name);

        role.Rename(cmd.Name);
        role.UpdatePermissions(cmd.PermissionCodes, _permissionPolicy.All);

        var permissions = role.Permissions.Select(p => p.Code).ToList().AsReadOnly();

        return new UpdateRoleOutput(role.Id, role.Name, role.IsActive, permissions);
    }
}
