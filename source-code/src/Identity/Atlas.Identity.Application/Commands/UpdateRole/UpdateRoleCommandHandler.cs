using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Roles;
using Atlas.Identity.Domain.Roles.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Commands.UpdateRole;

public sealed class UpdateRoleCommandHandler : IUpdateRoleCommandHandler
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRequestContext _requestContext;
    private readonly IPermissionCatalogCache _cache;
    private readonly IIdentityUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public UpdateRoleCommandHandler(
        IRoleRepository roleRepository,
        IRequestContext requestContext,
        IPermissionCatalogCache cache,
        IIdentityUnitOfWork uow
    )
    {
        _roleRepository = roleRepository;
        _requestContext = requestContext;
        _cache = cache;
        _uow = uow;
    }

    public async Task<UpdateRoleOutput> ExecuteAsync(UpdateRoleCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId ?? throw new TenantContextNotResolvedException();

        var role =
            await _roleRepository.GetByIdWithPermissionsAsync(cmd.RoleId, ct)
            ?? throw new RoleNotFoundException(cmd.RoleId);

        if (await _roleRepository.ExistsWithNameExcludingAsync(tenantId, cmd.Name, cmd.RoleId, ct))
            throw new RoleAlreadyExistsException(cmd.Name);

        role.Rename(cmd.Name);

        var (rolePermissions, permissionCodes) = await ResolvePermissionsAsync(cmd.PermissionCodes, ct);
        role.UpdatePermissions(rolePermissions);

        return new UpdateRoleOutput(role.Id, role.Name, role.IsActive, permissionCodes);
    }

    private async Task<(
        IReadOnlyList<RolePermission> RolePermissions,
        IReadOnlyList<string> Codes
    )> ResolvePermissionsAsync(IEnumerable<string> codes, CancellationToken ct)
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
        var resolvedCodes = found.Select(p => p.Code).ToList();

        return (rolePermissions, resolvedCodes);
    }
}
