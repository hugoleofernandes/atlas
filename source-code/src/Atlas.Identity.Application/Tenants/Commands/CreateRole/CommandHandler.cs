using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Tenants.Commands.CreateRole;

public sealed class CommandHandler : ICommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IRequestContext _requestContext;

    public CommandHandler(ITenantRepository tenantRepository, IRequestContext requestContext)
    {
        _tenantRepository = tenantRepository;
        _requestContext = requestContext;
    }

    public async Task<Output> ExecuteAsync(Command cmd, CancellationToken ct)
    {
        var tenantName = _requestContext.TenantName
            ?? throw new TenantContextNotResolvedException();

        var tenant = await _tenantRepository
            .GetByNameWithUsersInvitationsAndRolesAsync(tenantName, ct)
            ?? throw new TenantNotFoundException(tenantName);

        var role = tenant.AddCustomRole(cmd.Name, cmd.PermissionCodes);

        var permissions = role.Permissions.Select(p => p.Code).ToList().AsReadOnly();

        return new Output(role.Id, role.Name, permissions);
    }
}
