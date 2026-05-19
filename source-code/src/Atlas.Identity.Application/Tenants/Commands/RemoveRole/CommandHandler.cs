using Atlas.BuildingBlocks.Infrastructure.Workflows;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Domain.Entities.Tenants.Events;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.Exceptions;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Application.Tenants.Commands.RemoveRole;

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

        tenant.RemoveRole(cmd.RoleId);

        var wasPhysicallyDeleted = tenant.DomainEvents
            .OfType<RoleDeletedDomainEvent>()
            .Any();

        return new Output(wasPhysicallyDeleted);
    }
}
