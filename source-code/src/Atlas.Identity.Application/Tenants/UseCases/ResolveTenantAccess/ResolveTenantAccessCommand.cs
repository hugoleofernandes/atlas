namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

public sealed record ResolveTenantAccessCommand(
    string TenantName,
    string ExternalOid,
    string Email
);