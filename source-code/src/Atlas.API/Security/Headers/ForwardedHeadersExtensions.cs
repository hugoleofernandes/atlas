using Microsoft.AspNetCore.HttpOverrides;

namespace Atlas.API.Security.Headers;

/// <summary>
/// Configures forwarded headers support for scenarios where the application
/// is running behind a reverse proxy (e.g., Azure Container Apps, Kubernetes
/// ingress controllers, Nginx, AWS Load Balancers, or ngrok).
///
/// This enables correct handling of X-Forwarded-For and X-Forwarded-Proto
/// headers so the application can resolve the original client IP address
/// and request scheme (HTTP/HTTPS).
///
/// ⚠ Security Note:
/// This configuration assumes the application is deployed behind a trusted
/// reverse proxy. If the application is directly exposed to the internet,
/// accepting forwarded headers without restricting known proxies may allow
/// header spoofing. In such cases, explicitly configure KnownProxies or
/// KnownNetworks instead of trusting all sources.
///
/// This extension keeps Program.cs clean while centralizing proxy behavior.
/// </summary>

public static class ForwardedHeadersExtensions
{
    public static IApplicationBuilder UseForwardedHeadersDefaults(this IApplicationBuilder app)
    {
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto
        };

        return app.UseForwardedHeaders(options);
    }
}
