namespace Atlas.API.Security.OIDC;

/// <summary>
/// Defines application-wide constants used by the BFF, such as cookie names and
/// claim identifiers.
/// </summary>

public static class AuthConstants
{
    public const string SessionCookie = ".atlas-api.session";
    public const string Claim = "atlas-api_claim";
}
