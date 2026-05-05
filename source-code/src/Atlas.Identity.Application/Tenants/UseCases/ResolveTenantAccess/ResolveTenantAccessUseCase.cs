using Atlas.Identity.Application.Tenants.Abstractions;
using Atlas.Identity.Application.Users.Abstractions;
using Atlas.Identity.Domain.Users;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

public sealed class ResolveTenantAccessUseCase : IResolveTenantAccessUseCase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResolveTenantAccessUseCase(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResolveTenantAccessResult> ExecuteAsync(
        ResolveTenantAccessCommand command,
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

        // 🔹 Load tenant aggregate
        var tenant = await _tenantRepository.GetBySlugWithMembershipsAsync(tenantSlug, ct)
                     ?? throw new UnauthorizedAccessException("Tenant not found.");

        tenant.EnsureActive();

        // 🔹 Check user exists in tenant
        var existingUserId = tenant.Memberships.SingleOrDefault(x => x.Email == email)?.UserId;

        if (existingUserId is not null)
        {
            var membership = tenant.GetActiveMembershipByUserId(existingUserId.Value);

            return new ResolveTenantAccessResult(
                tenant.Id,
                tenant.Slug,
                existingUserId.Value,
                membership.Role);
        }

        // 🔹 if first login
        var newUser = new User(command.ExternalOid);

        await _userRepository.AddAsync(newUser, ct);

        var newMembership = tenant.BindUserToMembershipByEmail(newUser.Id, email);

        await _unitOfWork.SaveChangesAsync(ct);

        return new ResolveTenantAccessResult(
            tenant.Id,
            tenant.Slug,
            newUser.Id,
            newMembership.Role);
    }
}