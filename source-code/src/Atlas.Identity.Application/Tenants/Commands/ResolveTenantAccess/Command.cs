namespace Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

public sealed record Command(
    string TenantName,
    string ExternalOid,
    string Email
);