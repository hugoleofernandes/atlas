using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Aggregates.Tenants;
using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Commands.CreateRole;

public sealed class CreateRoleCommandHandler : ICreateRoleCommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IRequestContext _requestContext;
    private readonly IPermissionPolicy _permissionPolicy;
    private readonly IIdentityUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public CreateRoleCommandHandler(
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

    public async Task<CreateRoleOutput> ExecuteAsync(CreateRoleCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId
            ?? throw new TenantContextNotResolvedException();

        var tenant = await _tenantRepository
            .GetByIdWithRolesAsync(tenantId, ct)
            ?? throw new TenantNotFoundException(_requestContext.TenantName ?? tenantId.ToString());

        var role = tenant.AddRole(cmd.Name, cmd.PermissionCodes, _permissionPolicy.All);

        var permissions = role.Permissions.Select(p => p.Code).ToList().AsReadOnly();

        return new CreateRoleOutput(role.Id, role.Name, permissions);
    }
}
