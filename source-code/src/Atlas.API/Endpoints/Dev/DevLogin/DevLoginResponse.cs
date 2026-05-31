namespace Atlas.API.Endpoints.Dev.DevLogin;

public sealed record DevLoginResponse(
    Guid TenantId,
    string TenantName,
    Guid UserId,
    string Email,
    string RoleName,
    IReadOnlyList<string> Permissions
);
