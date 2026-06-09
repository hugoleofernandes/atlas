using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;

namespace Atlas.BuildingBlocks.AspNetCore.Oidc.Providers.Cognito;

/// <summary>
/// Configures OIDC options specific to AWS Cognito.
/// </summary>
public sealed class CognitoTenantConfigurator : IOidcTenantConfigurator
{
    public void Configure(
        OpenIdConnectOptions options,
        TenantOidcConfig tenantConfig,
        IConfiguration authCfg,
        string tenantName,
        string uiLocales)
    {
        throw new NotImplementedException("Cognito configurator not yet implemented.");
    }
}
