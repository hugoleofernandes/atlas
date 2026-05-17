using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

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
            options.Cookie.Name = AuthConstants.AuthCookie;
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

    /// <summary>
    /// Pre-loads OIDC metadata documents for all configured tenants in parallel
    /// immediately after the application starts listening, in a background task.
    ///
    /// By default, ASP.NET Core fetches each tenant's openid-configuration lazily
    /// on the first authenticated request — causing a noticeable delay on first login.
    /// This method eliminates that cold-start penalty by warming the cache eagerly,
    /// without blocking the startup path.
    /// </summary>
    public static WebApplication UseOidcMetadataWarmup(
        this WebApplication app,
        IConfiguration config)
    {
        var tenantNames = config
            .GetSection("Tenants")
            .Get<Dictionary<string, TenantConfig>>()
            ?.Keys.ToList() ?? [];

        if (tenantNames.Count == 0)
            return app;

        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Atlas.Security.OidcWarmup");

        var optionsMonitor = app.Services
            .GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>();

        // ApplicationStarted fires after the app is already listening —
        // so this never delays startup or the first response.
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            _ = Task.Run(async () =>
            {
                logger.LogInformation(
                    "Pre-loading OIDC metadata for {Count} tenant(s) in background...",
                    tenantNames.Count);

                // Fetch all tenants in parallel — each is an independent HTTP GET
                await Parallel.ForEachAsync(
                    tenantNames,
                    new ParallelOptions { MaxDegreeOfParallelism = tenantNames.Count },
                    async (name, ct) =>
                    {
                        try
                        {
                            var opts = optionsMonitor.Get(name);
                            if (opts.ConfigurationManager is null) return;

                            await opts.ConfigurationManager.GetConfigurationAsync(ct);

                            logger.LogInformation(
                                "OIDC metadata ready for tenant '{Tenant}'", name);
                        }
                        catch (Exception ex)
                        {
                            // Non-fatal: the middleware will retry on the next real request.
                            logger.LogWarning(ex,
                                "Failed to pre-load OIDC metadata for tenant '{Tenant}' — will retry on first request",
                                name);
                        }
                    });
            });
        });

        return app;
    }
}
