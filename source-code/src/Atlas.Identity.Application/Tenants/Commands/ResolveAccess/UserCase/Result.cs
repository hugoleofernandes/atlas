namespace Atlas.Identity.Application.Tenants.Commands.ResolveAccess.UserCase;

public sealed record Result(
    Guid TenantId,
    string TenantName,
    Guid UserId,
    string Role
);