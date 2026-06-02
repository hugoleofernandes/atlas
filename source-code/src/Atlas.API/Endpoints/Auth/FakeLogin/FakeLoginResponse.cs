namespace Atlas.API.Endpoints.Auth.FakeLogin;

public sealed record FakeLoginResponse(
    Guid TenantId,
    string TenantName,
    Guid UserId,
    string Email,
    string RoleName,
    IReadOnlyList<string> Permissions
);
