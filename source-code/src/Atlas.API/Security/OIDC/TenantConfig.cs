namespace Atlas.API.Security.OIDC;

/// <summary>
/// Represents the strongly-typed configuration for a single OpenID Connect (OIDC).
/// Each information corresponds to a tenant/identity provider configured 
/// under the Tenants section in appsettings.json.
/// </summary>


public class TenantConfig
{
    public required string Authority { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}
