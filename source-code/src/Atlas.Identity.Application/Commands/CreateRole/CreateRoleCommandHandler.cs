using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Permissions;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Tenants._Roles;
using Atlas.Identity.Domain.Tenants._Roles.Exceptions;
using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Commands.CreateRole;

public sealed class CreateRoleCommandHandler : ICreateRoleCommandHandler
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRequestContext _requestContext;
    private readonly IPermissionPolicy _permissionPolicy;
    private readonly IIdentityUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        IRequestContext requestContext,
        IPermissionPolicy permissionPolicy,
        IIdentityUnitOfWork uow)
    {
        _roleRepository = roleRepository;
        _requestContext = requestContext;
        _permissionPolicy = permissionPolicy;
        _uow = uow;
    }

    public async Task<CreateRoleOutput> ExecuteAsync(CreateRoleCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId
            ?? throw new TenantContextNotResolvedException();

        if (await _roleRepository.ExistsWithNameAsync(tenantId, cmd.Name, ct))
            throw new RoleAlreadyExistsException(cmd.Name);

        var permissions = PermissionResolution.Resolve(cmd.PermissionCodes, _permissionPolicy);
        var role = Role.Create(tenantId, cmd.Name, permissions);

        await _roleRepository.AddAsync(role, ct);

        var permissionCodes = role.Permissions.Select(p => p.Code).ToList().AsReadOnly();

        return new CreateRoleOutput(role.Id, role.Name, role.IsActive, permissionCodes);
    }
}
