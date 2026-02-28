using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Domain.Entities;

namespace Atlas.Identity.Application.UseCases.AuthorizeTenantLogin;

public sealed class AuthorizeTenantLoginUseCase : IAuthorizeTenantLoginUseCase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuthorizeTenantLoginUseCase(
        ITenantRepository tenantRepository,
        IIdentityUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthorizeTenantLoginResult> ExecuteAsync(
        AuthorizeTenantLoginCommand command,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.TenantSlug))
            throw new ArgumentException("TenantSlug is required.");

        if (string.IsNullOrWhiteSpace(command.ExternalOid))
            throw new ArgumentException("ExternalOid is required.");

        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ArgumentException("Email is required.");

        var tenantSlug = command.TenantSlug.ToLowerInvariant();
        var email = command.Email.ToLowerInvariant();

        // 🔹 Buscar Tenant com memberships
        var tenant = await _tenantRepository
            .GetBySlugWithMembershipsAsync(tenantSlug, ct);

        if (tenant is null || !tenant.IsActive)
            throw new UnauthorizedAccessException("Tenant not found or inactive.");

        // 🔹 Tentar login normal (OID já vinculado)
        var existingUser = await _userRepository
            .GetByExternalIdAsync(command.ExternalOid, ct);

        if (existingUser is not null)
        {
            if (!existingUser.IsActive)
                throw new UnauthorizedAccessException("User is inactive.");

            var membership = tenant.FindMembershipByUser(existingUser.Id);

            if (membership is null)
                throw new UnauthorizedAccessException("User not linked to this tenant.");

            return new AuthorizeTenantLoginResult(
                tenant.Id,
                tenant.Slug,
                existingUser.Id,
                membership.Role);
        }

        // 🔹 Primeiro login (fallback por email)
        var invitedMembership = tenant.FindMembershipByEmail(email);

        if (invitedMembership is null)
            throw new UnauthorizedAccessException("User not invited to this tenant.");

        // 🔹 Criar novo IdentityUser com OID
        var newUser = new IdentityUser(command.ExternalOid);

        await _userRepository.AddAsync(newUser, ct);

        // 🔹 Vincular membership ao novo IdentityUser
        invitedMembership.BindIdentityUser(newUser.Id);

        await _unitOfWork.SaveChangesAsync(ct);

        return new AuthorizeTenantLoginResult(
            tenant.Id,
            tenant.Slug,
            newUser.Id,
            invitedMembership.Role);
    }
}