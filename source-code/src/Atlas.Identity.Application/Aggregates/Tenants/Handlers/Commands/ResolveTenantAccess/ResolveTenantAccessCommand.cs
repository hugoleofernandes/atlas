namespace Atlas.Identity.Application.Aggregates.Tenants.Handlers.Commands.ResolveTenantAccess;

public sealed record ResolveTenantAccessCommand(
    string TenantName,
    string ExternalOid,
    string Email
);