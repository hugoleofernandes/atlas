using Atlas.BuildingBlocks.Infrastructure.Workflows;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.Exceptions;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Application.Tenants.Commands.CreateRole;

public sealed class CommandHandler : CommandHandlerBase<Command, Output>, ICommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IRequestContext _requestContext;

    public CommandHandler(
        ITenantRepository tenantRepository,
        IRequestContext requestContext,
        ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        _tenantRepository = tenantRepository;
        _requestContext = requestContext;
    }

    protected override async Task<Output> HandleAsync(Command cmd, CancellationToken ct)
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
