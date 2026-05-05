namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

public sealed record ResolveTenantAccessCommand(
    string TenantSlug,
    string ExternalOid,
    string Email
);