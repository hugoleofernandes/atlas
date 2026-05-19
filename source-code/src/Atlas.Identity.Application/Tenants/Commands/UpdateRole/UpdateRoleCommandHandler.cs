using Atlas.BuildingBlocks.Infrastructure.Workflows;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.Exceptions;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Application.Tenants.Commands.UpdateRole;

public sealed class UpdateRoleCommandHandler : CommandHandlerBase<UpdateRoleCommand, UpdateRoleOutput>, IUpdateRoleCommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IRequestContext _requestContext;

    public UpdateRoleCommandHandler(
        ITenantRepository tenantRepository,
        IRequestContext requestContext,
        ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        _tenantRepository = tenantRepository;
        _requestContext = requestContext;
    }

    protected override async Task<UpdateRoleOutput> HandleAsync(UpdateRoleCommand cmd, CancellationToken ct)
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
