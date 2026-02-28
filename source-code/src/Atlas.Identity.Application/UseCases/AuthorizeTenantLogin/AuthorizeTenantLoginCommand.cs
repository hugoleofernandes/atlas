namespace Atlas.Identity.Application.UseCases.AuthorizeTenantLogin;

public sealed record AuthorizeTenantLoginCommand(
    string TenantSlug,
    string ExternalOid,
    string Email
);