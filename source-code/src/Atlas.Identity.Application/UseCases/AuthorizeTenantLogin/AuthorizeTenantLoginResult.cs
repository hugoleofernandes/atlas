namespace Atlas.Identity.Application.UseCases.AuthorizeTenantLogin;

public sealed record AuthorizeTenantLoginResult(
    Guid TenantId,
    string TenantSlug,
    Guid IdentityUserId,
    string Role
);