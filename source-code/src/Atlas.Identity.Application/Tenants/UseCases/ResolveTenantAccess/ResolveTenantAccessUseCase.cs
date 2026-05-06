using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Abstractions;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

public sealed class ResolveTenantAccessUseCase : IResolveTenantAccessUseCase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public ResolveTenantAccessUseCase(
        ITenantRepository tenantRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResolveTenantAccessResult> ExecuteAsync(
    ResolveTenantAccessCommand command,
    CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.TenantName))
            throw new ArgumentException("TenantName is required.");

        if (string.IsNullOrWhiteSpace(command.ExternalOid))
            throw new ArgumentException("ExternalOid is required.");

        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ArgumentException("Email is required.");

        var tenant = await _tenantRepository
            .GetByNameWithUsersAndInvitationsAsync(command.TenantName.ToLowerInvariant(), ct)
            ?? throw new UnauthorizedAccessException("Tenant not found.");

        var user = tenant.ResolveAccess(
            command.ExternalOid,
            command.Email);

        await _unitOfWork.SaveChangesAsync(ct);

        return new ResolveTenantAccessResult(
            tenant.Id,
            tenant.Name,
            user.Id,
            user.Role);
    }
}