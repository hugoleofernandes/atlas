namespace Atlas.API.Security.OIDC;

/// <summary>
/// Defines application-wide constants used by the BFF, such as cookie names and
/// claim identifiers.
/// </summary>

public static class AuthConstants
{
    public const string AuthCookie = ".atlas-api.session";
    public const string TenantHintCookie = "atlas-tenant-hint-cookie";
    //public const string Claim = "atlas-api_claim";
}

public static class ClaimConstants
{
    public const string TenantName = "atlas_tenant_name";
    public const string TenantId = "atlas_tenant_id";
    public const string UserId = "atlas_user_id";
    //public const string Claim = "atlas-api_claim";
}
