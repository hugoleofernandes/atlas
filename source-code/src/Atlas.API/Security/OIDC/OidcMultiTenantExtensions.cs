using Microsoft.AspNetCore.Authentication.Cookies;

namespace Atlas.API.Security.OIDC;

/// <summary>
/// Registers the authentication pipeline for the BFF (Backend-for-Frontend),
/// including the session cookie and all OpenID Connect (OIDC) providers (labs).
///
/// This component reads the Tenants section from configuration and
/// dynamically registers one OIDC scheme for each tenant defined. 
/// 
/// The goal is to support a multi-tenant authentication architecture where
/// new tenant can be added simply by updating the appsettings.json — without
/// modifying Program.cs or recompiling the application.
/// </summary>

public static class OidcMultiTenantExtensions
{
    public static IServiceCollection AddOidcMultiTenantAuthentication(
        this IServiceCollection services,
        IConfiguration config)
    {
        var authenticationCfg = config.GetSection("Authentication");
        var uiLocales = authenticationCfg["UiLocales"] ?? "pt-BR";

        var tenants = config.GetSection("Tenants").Get<Dictionary<string, TenantConfig>>()?.ToList() ?? [];

        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });

        authBuilder.AddCookie(options =>
        {
            options.Cookie.Name = AuthConstants.SessionCookie;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.None;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

        foreach (var (name, cfg) in tenants)
        {
            authBuilder.AddOpenIdConnect(name, options =>
            {
                OidcMultiTenantConfigurator.Configure(options, authenticationCfg, cfg, uiLocales, name);
            });
        }

        return services;
    }
}
