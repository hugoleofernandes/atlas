using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.BuildingBlocks.AspNetCore.Oidc;

public static class MultiTenantOidcExtensions
{
    /// <summary>
    /// Registers cookie + per-tenant OIDC schemes.
    /// The provider-specific details are delegated to the given <see cref="IOidcTenantConfigurator"/>.
    /// </summary>
    public static IServiceCollection AddMultiTenantOidc(
        this IServiceCollection services,
        IConfiguration config,
        IOidcTenantConfigurator configurator,
        string sessionCookieName)
    {
        var authCfg = config.GetSection("Authentication");
        var uiLocales = authCfg["UiLocales"] ?? "pt-BR";
        var tenants = config.GetSection("Tenants")
            .Get<Dictionary<string, TenantOidcConfig>>()?.ToList() ?? [];

        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });

        authBuilder.AddCookie(options =>
        {
            options.Cookie.Name = sessionCookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.None;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

        foreach (var (name, tenantCfg) in tenants)
        {
            authBuilder.AddOpenIdConnect(name, options =>
            {
                configurator.Configure(options, tenantCfg, authCfg, name, uiLocales);
            });
        }

        return services;
    }

    /// <summary>
    /// Pre-loads OIDC metadata for all tenants in background after startup,
    /// eliminating the cold-start delay on the first login request.
    /// </summary>
    public static WebApplication UseOidcMetadataWarmup(
        this WebApplication app,
        IConfiguration config)
    {
        var tenantNames = config
            .GetSection("Tenants")
            .Get<Dictionary<string, TenantOidcConfig>>()
            ?.Keys.ToList() ?? [];

        if (tenantNames.Count == 0)
            return app;

        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Atlas.Security.OidcWarmup");

        var optionsMonitor = app.Services
            .GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>();

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            _ = Task.Run(async () =>
            {
                logger.LogInformation(
                    "Pre-loading OIDC metadata for {Count} tenant(s) in background...",
                    tenantNames.Count);

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
