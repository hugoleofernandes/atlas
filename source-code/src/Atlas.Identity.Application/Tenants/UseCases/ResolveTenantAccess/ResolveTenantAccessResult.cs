namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

public sealed record ResolveTenantAccessResult(
    Guid TenantId,
    string TenantSlug,
    Guid UserId,
    string Role
);