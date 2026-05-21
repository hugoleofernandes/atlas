namespace Atlas.BuildingBlocks.AspNetCore.Oidc;

/// <summary>
/// Strongly-typed configuration for a single OIDC tenant.
/// Mapped from the Tenants section in appsettings.json.
/// </summary>
public class TenantOidcConfig
{
    public required string Authority { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}
