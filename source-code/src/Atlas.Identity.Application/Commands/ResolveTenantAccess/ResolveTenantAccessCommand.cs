namespace Atlas.Identity.Application.Commands.ResolveTenantAccess;

public sealed record ResolveTenantAccessCommand(
    Guid   TenantId,
    string TenantName,
    string ExternalOid,
    string Email
);
