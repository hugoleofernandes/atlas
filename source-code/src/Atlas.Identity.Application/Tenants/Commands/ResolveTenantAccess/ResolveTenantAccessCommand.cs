namespace Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

public sealed record ResolveTenantAccessCommand(
    string TenantName,
    string ExternalOid,
    string Email
);