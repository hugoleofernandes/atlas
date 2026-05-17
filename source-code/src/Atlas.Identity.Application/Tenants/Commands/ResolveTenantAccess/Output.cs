namespace Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

public sealed record Output(
    Guid TenantId,
    string TenantName,
    Guid UserId,
    string Role
);