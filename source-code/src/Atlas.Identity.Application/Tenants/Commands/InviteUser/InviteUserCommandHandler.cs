using Atlas.BuildingBlocks.Infrastructure.Workflows;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.Exceptions;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Identity.Application.Tenants.Commands.InviteUser;

public sealed class InviteUserCommandHandler : CommandHandlerBase<InviteUserCommand, InviteUserOutput>, IInviteUserCommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IRequestContext _requestContext;
    private readonly TimeSpan _defaultTtl;

    public InviteUserCommandHandler(
        ITenantRepository tenantRepository,
        IRequestContext requestContext,
        IOptions<InvitationSettings> options,
        ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        _tenantRepository = tenantRepository;
        _requestContext = requestContext;
        _defaultTtl = TimeSpan.FromDays(options.Value.TtlDays);
    }

    protected override async Task<InviteUserOutput> HandleAsync(InviteUserCommand cmd, CancellationToken ct)
    {
        var tenantName = _requestContext.TenantName
            ?? throw new TenantContextNotResolvedException();

        var tenant = await _tenantRepository
            .GetByNameWithUsersInvitationsAndRolesAsync(tenantName, ct)
            ?? throw new TenantNotFoundException(tenantName);

        var invitation = tenant.InviteUser(
            Email.Create(cmd.Email),
            cmd.RoleId,
            InvitationTtl.Create(_defaultTtl));

        var role = tenant.Roles.Single(r => r.Id == cmd.RoleId);

        return new InviteUserOutput(
            invitation.Id,
            invitation.Email.Value,
            role.Id,
            role.Name,
            invitation.ExpiresAt);
    }
}
