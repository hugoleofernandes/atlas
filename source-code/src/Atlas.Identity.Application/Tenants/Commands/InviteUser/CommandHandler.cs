using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Domain.Exceptions;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.Options;

namespace Atlas.Identity.Application.Tenants.Commands.InviteUser;

public sealed class CommandHandler : ICommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IRequestContext _requestContext;
    private readonly TimeSpan _defaultTtl;

    public CommandHandler(
        ITenantRepository tenantRepository,
        IRequestContext requestContext,
        IOptions<InvitationSettings> options)
    {
        _tenantRepository = tenantRepository;
        _requestContext = requestContext;
        _defaultTtl = TimeSpan.FromDays(options.Value.TtlDays);
    }

    public async Task<Output> ExecuteAsync(Command cmd, CancellationToken ct)
    {
        var tenantName = _requestContext.TenantName
            ?? throw new InvalidOperationException("TenantName not available in request context.");

        var tenant = await _tenantRepository
            .GetByNameWithUsersInvitationsAndRolesAsync(tenantName, ct)
            ?? throw new TenantNotFoundException(tenantName);

        var invitation = tenant.InviteUser(
            Email.Create(cmd.Email),
            cmd.RoleId,
            InvitationTtl.Create(_defaultTtl));

        var role = tenant.Roles.Single(r => r.Id == cmd.RoleId);

        return new Output(
            invitation.Id,
            invitation.Email.Value,
            role.Id,
            role.Name,
            invitation.ExpiresAt);
    }
}
