using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;

namespace Atlas.BuildingBlocks.AspNetCore.Oidc.Providers.EntraId;

/// <summary>
/// Configures OIDC options specific to Microsoft Entra ID (formerly Azure AD).
/// Handles PKCE, scopes, callback paths, security hardening (no KMSI),
/// tenant hint cookie and locale parameters.
/// </summary>
public sealed class EntraIdTenantConfigurator : IOidcTenantConfigurator
{
    private readonly string _tenantHintCookieName;

    public EntraIdTenantConfigurator(string tenantHintCookieName)
    {
        _tenantHintCookieName = tenantHintCookieName;
    }

    public void Configure(
        OpenIdConnectOptions options,
        TenantOidcConfig tenantConfig,
        IConfiguration authCfg,
        string tenantName,
        string uiLocales)
    {
        options.Authority = tenantConfig.Authority;
        options.ClientId = tenantConfig.ClientId;
        options.ClientSecret = tenantConfig.ClientSecret;

        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = false;
        options.BackchannelTimeout = TimeSpan.FromSeconds(10);

        options.Scope.Clear();
        var rawScopes = authCfg["Scopes"] ?? "openid profile email";
        foreach (var scope in rawScopes.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            options.Scope.Add(scope);

        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

        options.CallbackPath = authCfg["CallbackPath"]?.Replace("{tenant}", tenantName);

        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = ctx =>
            {
                ctx.HttpContext.Response.Cookies.Append(_tenantHintCookieName, tenantName, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/"
                });

                var authority = tenantConfig.Authority;
                var issuer = authority.Replace("/v2.0", "/oauth2/v2.0/authorize");

                ctx.ProtocolMessage.AuthorizationEndpoint = authority;
                ctx.ProtocolMessage.IssuerAddress = issuer;
                ctx.ProtocolMessage.ClientId = tenantConfig.ClientId;
                ctx.ProtocolMessage.SetParameter("client_secret", tenantConfig.ClientSecret);

                ctx.ProtocolMessage.Prompt = "login";
                ctx.ProtocolMessage.SetParameter("login_hint", "");
                ctx.ProtocolMessage.SetParameter("domain_hint", "none");
                ctx.ProtocolMessage.SetParameter("max_age", "0");
                ctx.ProtocolMessage.SetParameter("remember", "false");
                ctx.ProtocolMessage.SetParameter("suppress_prompt", "true");
                ctx.ProtocolMessage.SetParameter("auth_method", "refresh_session");
                ctx.ProtocolMessage.SetParameter("disable_kmsi", "1");
                ctx.ProtocolMessage.SetParameter("ui_locales", uiLocales);
                ctx.ProtocolMessage.SetParameter("mkt", uiLocales);
                ctx.ProtocolMessage.ResponseMode = "query";

                return Task.CompletedTask;
            },

            OnRedirectToIdentityProviderForSignOut = ctx =>
            {
                ctx.ProtocolMessage.SetParameter("ui_locales", uiLocales);
                ctx.ProtocolMessage.SetParameter("mkt", uiLocales);

                var email = ctx.HttpContext.User.FindFirst("preferred_username")?.Value;
                if (!string.IsNullOrWhiteSpace(email))
                    ctx.ProtocolMessage.SetParameter("logout_hint", email);

                return Task.CompletedTask;
            },

            OnTokenValidated = ctx =>
            {
                var oid = ctx.Principal?
                    .FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")
                    ?.Value;

                var email = ctx.Principal?
                    .FindFirst("preferred_username")
                    ?.Value;

                if (string.IsNullOrWhiteSpace(oid) || string.IsNullOrWhiteSpace(email))
                {
                    ctx.Fail("Missing required identity claims.");
                    return Task.CompletedTask;
                }

                if (ctx.Principal?.Identity is ClaimsIdentity identity)
                    identity.AddClaim(new Claim("tenant_name", tenantName));

                return Task.CompletedTask;
            }
        };
    }
}
