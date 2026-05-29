namespace Atlas.Identity.Application.Commands.ResolveTenantAccess;

public sealed record ResolveTenantAccessCommand(
    string TenantName,
    string ExternalOid,
    string Email
);