namespace Atlas.API.Security.OIDC;

/// <summary>
/// Defines application-wide constants used by the BFF, such as cookie names.
/// </summary>
public static class AuthConstants
{
    public const string AuthCookie = ".atlas-api.session";
    public const string TenantHintCookie = "atlas-tenant-hint-cookie";
}
