namespace Atlas.Identity.Application.Tenants.Commands.ResolveAccess.UserCase;

public sealed record Command(
    string TenantName,
    string ExternalOid,
    string Email
);