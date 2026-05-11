namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

public sealed record Command(
    string TenantName,
    string ExternalOid,
    string Email
);