namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

public sealed record Output(
    Guid TenantId,
    string TenantName,
    Guid UserId,
    string Role
);