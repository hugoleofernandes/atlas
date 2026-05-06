namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

public sealed record ResolveTenantAccessResult(
    Guid TenantId,
    string TenantName,
    Guid UserId,
    string Role
);