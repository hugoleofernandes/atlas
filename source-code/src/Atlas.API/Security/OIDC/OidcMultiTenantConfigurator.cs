using Atlas.Identity.Application.UseCases.AuthorizeTenantLogin;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;

namespace Atlas.API.Security.OIDC;

/// <summary>
/// Centralizes all OpenID Connect configuration for each tenant.
///
/// Responsibilities:
/// - Point the BFF to the correct identity provider for each tenant.
/// - Generate the correct callback and logout URLs dynamically.
/// - Apply security hardening (PKCE, no KMSI, no silent login, forced prompt).
/// - Emit custom claims such as "atlas-api_claim".
/// - Write cross-tenant cookies to ensure logout and token flow.
/// - Allow dynamic onboarding of new tenants by updating appsettings.json only.
/// </summary>
public static class OidcMultiTenantConfigurator
{
    public static void Configure(
        OpenIdConnectOptions options,
        IConfiguration authCfg,
        TenantConfig tenantConfig,
        string uiLocales,
        string tenantName)
    {
        //
        // ==================== BASIC OIDC CONFIG ====================
        //
        options.Authority = tenantConfig.Authority;
        options.ClientId = tenantConfig.ClientId;
        options.ClientSecret = tenantConfig.ClientSecret;

        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;

        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = false;


        //
        // ==================== SCOPES ====================
        //
        options.Scope.Clear();

        var rawScopes = authCfg["Scopes"] ?? "openid profile email";
        foreach (var scope in rawScopes.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            options.Scope.Add(scope);


        //
        // ==================== CLAIM RULES ====================
        //
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;


        //
        // ==================== CALLBACKS ====================
        //
        options.CallbackPath = authCfg["CallbackPath"]?.Replace("{tenant}", tenantName);
        //options.SignedOutCallbackPath = authCfg["SignedOutCallbackPath"]; // use manual callback because is SPA


        //
        // ==================== EVENTS ====================
        //
        options.Events = new OpenIdConnectEvents
        {
            //
            // ----- LOGIN REQUEST -----
            //
            OnRedirectToIdentityProvider = ctx =>
            {
                var http = ctx.HttpContext;
                var tenantCurrent = ctx.Scheme.Name.ToLowerInvariant();

                // Cookie with tenant info for the logout flow and token validation
                http.Response.Cookies.Append(AuthConstants.TenantHintCookie, tenantCurrent, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/"
                });

                //
                // ----- Ajuste de endpoints do Hosted UI -----
                //
                var authority = tenantConfig.Authority;
                var issuer = authority.Replace("/v2.0", "/oauth2/v2.0/authorize");

                ctx.ProtocolMessage.AuthorizationEndpoint = authority;
                ctx.ProtocolMessage.IssuerAddress = issuer;
                ctx.ProtocolMessage.ClientId = tenantConfig.ClientId;
                ctx.ProtocolMessage.SetParameter("client_secret", tenantConfig.ClientSecret);

                //
                // ----- Forçar login SEM "Keep me signed in" -----
                //
                ctx.ProtocolMessage.Prompt = "login";
                ctx.ProtocolMessage.SetParameter("login_hint", "");
                ctx.ProtocolMessage.SetParameter("domain_hint", "none");
                ctx.ProtocolMessage.SetParameter("max_age", "0");
                ctx.ProtocolMessage.SetParameter("remember", "false");
                ctx.ProtocolMessage.SetParameter("suppress_prompt", "true");
                ctx.ProtocolMessage.SetParameter("auth_method", "refresh_session");
                ctx.ProtocolMessage.SetParameter("disable_kmsi", "true");
                ctx.ProtocolMessage.SetParameter("disable_kmsi", "1");

                //
                // ----- Idioma -----
                //
                ctx.ProtocolMessage.SetParameter("ui_locales", uiLocales);
                ctx.ProtocolMessage.SetParameter("mkt", uiLocales);

                ctx.ProtocolMessage.ResponseMode = "query";

                Console.WriteLine($"🔹 Redirecting to Hosted UI → Scheme={tenantCurrent}");

                return Task.CompletedTask;
            },

            //
            // ----- LOGOUT REQUEST -----
            //
            OnRedirectToIdentityProviderForSignOut = ctx =>
            {
                ctx.ProtocolMessage.SetParameter("ui_locales", uiLocales);
                ctx.ProtocolMessage.SetParameter("mkt", uiLocales);

                var email = ctx.HttpContext.User.FindFirst("preferred_username")?.Value;

                if (!string.IsNullOrWhiteSpace(email))
                {
                    ctx.ProtocolMessage.SetParameter("logout_hint", email);
                }

                return Task.CompletedTask;
            },

            //
            // ----- TOKEN RECEIVED -----
            //
            OnTokenValidated = async ctx =>
            {
                var http = ctx.HttpContext;

                var tenantSlug = ctx.Scheme.Name.ToLowerInvariant();

                var oid = ctx.Principal?
                    .FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")
                    ?.Value;

                var email = ctx.Principal?
                    .FindFirst("preferred_username")
                    ?.Value;

                if (string.IsNullOrWhiteSpace(oid) || string.IsNullOrWhiteSpace(email))
                {
                    ctx.Fail("Missing required identity claims.");
                    return;
                }

                var useCase = http.RequestServices
                    .GetRequiredService<IAuthorizeTenantLoginUseCase>();

                AuthorizeTenantLoginResult result;

                try
                {
                    result = await useCase.ExecuteAsync(
                        new AuthorizeTenantLoginCommand(
                            tenantSlug,
                            oid,
                            email),
                        ctx.HttpContext.RequestAborted);
                }
                catch (UnauthorizedAccessException ex)
                {
                    ctx.Fail(ex.Message);
                    return;
                }

                if (ctx.Principal?.Identity is ClaimsIdentity identity)
                {
                    identity.AddClaim(new Claim("tenant_id", result.TenantId.ToString()));
                    identity.AddClaim(new Claim("tenant_slug", result.TenantSlug));
                    identity.AddClaim(new Claim("user_id", result.IdentityUserId.ToString()));
                    identity.AddClaim(new Claim(ClaimTypes.Role, result.Role));
                }
            }
        };
    }
}
