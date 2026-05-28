using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.Entities.Tenants.Invitations;
using Atlas.Identity.Domain.Exceptions;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.Options;

namespace Atlas.Identity.Application.Tenants.Commands.InviteUser;

public sealed class InviteUserCommandHandler : IInviteUserCommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IRequestContext _requestContext;
    private readonly IIdentityUnitOfWork _uow;
    private readonly TimeSpan _defaultTtl;

    public IUnitOfWork UnitOfWork => _uow;

    public InviteUserCommandHandler(
        ITenantRepository tenantRepository,
        IRequestContext requestContext,
        IIdentityUnitOfWork uow,
        IOptions<InvitationSettings> options)
    {
        _tenantRepository = tenantRepository;
        _requestContext = requestContext;
        _uow = uow;
        _defaultTtl = TimeSpan.FromDays(options.Value.TtlDays);
    }

    public async Task<InviteUserOutput> ExecuteAsync(InviteUserCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId
            ?? throw new TenantContextNotResolvedException();

        var tenant = await _tenantRepository
            .GetByIdWithUsersActiveInvitationsAndRolesAsync(tenantId, ct)
            ?? throw new TenantNotFoundException(_requestContext.TenantName ?? tenantId.ToString());

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
