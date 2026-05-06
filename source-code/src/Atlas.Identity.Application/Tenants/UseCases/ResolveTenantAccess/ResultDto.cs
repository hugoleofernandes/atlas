namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

public sealed record ResultDto(
    Guid TenantId,
    string TenantName,
    Guid UserId,
    string Role
);