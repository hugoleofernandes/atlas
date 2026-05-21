using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Domain.Exceptions;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

public sealed class ResolveTenantAccessCommandHandler : IResolveTenantAccessCommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IIdentityUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public ResolveTenantAccessCommandHandler(
        ITenantRepository tenantRepository,
        IIdentityUnitOfWork uow)
    {
        _tenantRepository = tenantRepository;
        _uow = uow;
    }

    public async Task<ResolveTenantAccessOutput> ExecuteAsync(ResolveTenantAccessCommand cmd, CancellationToken ct)
    {
        var tenant = await _tenantRepository
            .GetByNameWithUsersInvitationsAndRolesAsync(
                cmd.TenantName.ToLowerInvariant(), ct)
            ?? throw new TenantNotFoundException(cmd.TenantName);

        var email = Email.Create(cmd.Email);
        var existingExternalId = tenant.Users
            .FirstOrDefault(u => u.Email.Value == email.Value && u.IsActive)
            ?.ExternalId.Value;

        var user = tenant.ResolveAccess(
            ExternalId.Create(existingExternalId ?? cmd.ExternalOid),
            email);

        var role = tenant.Roles.Single(r => r.Id == user.RoleId);
        var permissions = role.Permissions.Select(p => p.Code).ToList().AsReadOnly();

        return new ResolveTenantAccessOutput(
            tenant.Id,
            tenant.Name,
            user.Id,
            role.Id,
            role.Name,
            permissions);
    }
}
