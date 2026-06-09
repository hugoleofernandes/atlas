using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;

namespace Atlas.BuildingBlocks.AspNetCore.Oidc;

/// <summary>
/// Abstracts provider-specific OIDC configuration.
/// Implement this to support a new identity provider (Entra ID, Cognito, Auth0, etc.)
/// without changing the multi-tenant registration machinery.
/// </summary>
public interface IOidcTenantConfigurator
{
    void Configure(
        OpenIdConnectOptions options,
        TenantOidcConfig tenantConfig,
        IConfiguration authCfg,
        string tenantName,
        string uiLocales);
}
