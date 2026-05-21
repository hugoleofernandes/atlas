using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.Exceptions;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Commands.CreateRole;

public sealed class CreateRoleCommandHandler : ICreateRoleCommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IRequestContext _requestContext;
    private readonly IIdentityUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public CreateRoleCommandHandler(
        ITenantRepository tenantRepository,
        IRequestContext requestContext,
        IIdentityUnitOfWork uow)
    {
        _tenantRepository = tenantRepository;
        _requestContext = requestContext;
        _uow = uow;
    }

    public async Task<CreateRoleOutput> ExecuteAsync(CreateRoleCommand cmd, CancellationToken ct)
    {
        var tenantName = _requestContext.TenantName
            ?? throw new TenantContextNotResolvedException();

        var tenant = await _tenantRepository
            .GetByNameWithRolesAsync(tenantName, ct)
            ?? throw new TenantNotFoundException(tenantName);

        var role = tenant.AddCustomRole(cmd.Name, cmd.PermissionCodes);

        var permissions = role.Permissions.Select(p => p.Code).ToList().AsReadOnly();

        return new CreateRoleOutput(role.Id, role.Name, permissions);
    }
}
