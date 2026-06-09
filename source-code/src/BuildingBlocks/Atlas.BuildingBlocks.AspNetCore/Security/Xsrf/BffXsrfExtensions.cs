using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.BuildingBlocks.AspNetCore.Security.Xsrf;

public static class BffXsrfExtensions
{
    public static IServiceCollection AddBffXsrf(this IServiceCollection services)
    {
        services.AddAntiforgery(options =>
        {
            options.Cookie.Name = BffXsrfDefaults.CookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.None;
            options.HeaderName = BffXsrfDefaults.HeaderName;
        });

        services.Configure<BffXsrfOptions>(_ => { });

        return services;
    }

    public static IApplicationBuilder UseBffXsrf(this IApplicationBuilder app)
        => app.UseMiddleware<BffXsrfMiddleware>();

    public static string CreateAndStoreBffXsrfToken(this IAntiforgery antiforgery, HttpContext context)
    {
        var tokens = antiforgery.GetAndStoreTokens(context);
        return tokens.RequestToken
            ?? throw new InvalidOperationException("Unable to generate an XSRF request token.");
    }
}
