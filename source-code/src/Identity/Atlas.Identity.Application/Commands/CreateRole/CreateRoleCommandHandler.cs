using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Tenants._Roles;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.Identity.Domain.Tenants._Roles.Exceptions;
using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Commands.CreateRole;

public sealed class CreateRoleCommandHandler : ICreateRoleCommandHandler
{
    private readonly IRoleRepository         _roleRepository;
    private readonly IRequestContext         _requestContext;
    private readonly IPermissionCatalogCache _cache;
    private readonly IIdentityUnitOfWork     _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public CreateRoleCommandHandler(
        IRoleRepository         roleRepository,
        IRequestContext         requestContext,
        IPermissionCatalogCache cache,
        IIdentityUnitOfWork     uow)
    {
        _roleRepository = roleRepository;
        _requestContext = requestContext;
        _cache          = cache;
        _uow            = uow;
    }

    public async Task<CreateRoleOutput> ExecuteAsync(CreateRoleCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId
            ?? throw new TenantContextNotResolvedException();

        if (await _roleRepository.ExistsWithNameAsync(tenantId, cmd.Name, ct))
            throw new RoleAlreadyExistsException(cmd.Name);

        var (rolePermissions, permissionCodes) = await ResolvePermissionsAsync(cmd.PermissionCodes, ct);
        var role = Role.Create(tenantId, cmd.Name, rolePermissions);

        await _roleRepository.AddAsync(role, ct);

        return new CreateRoleOutput(role.Id, role.Name, role.IsActive, permissionCodes);
    }

    private async Task<(IReadOnlyList<RolePermission> RolePermissions, IReadOnlyList<string> Codes)> ResolvePermissionsAsync(
        IEnumerable<string> codes,
        CancellationToken ct)
    {
        var codeList = codes.Distinct(StringComparer.Ordinal).ToList();

        if (codeList.Count == 0)
            return ([], []);

        var all = await _cache.GetAllActiveAsync(ct);
        var found = all.Where(p => codeList.Contains(p.Code, StringComparer.Ordinal)).ToList();

        var foundCodes = found.Select(p => p.Code).ToHashSet(StringComparer.Ordinal);
        var unknown = codeList.Where(c => !foundCodes.Contains(c)).ToList();

        if (unknown.Count > 0)
            throw new RoleWithInvalidPermissionException(unknown);

        var rolePermissions = found.Select(p => RolePermission.Of(p.Id)).ToList();
        var resolvedCodes   = found.Select(p => p.Code).ToList();

        return (rolePermissions, resolvedCodes);
    }
}
