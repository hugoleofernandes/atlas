using Atlas.API.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Atlas.API.Tests.Infrastructure;

/// <summary>
/// Fake authentication handler for integration tests.
/// Reads identity from the X-Test-Identity request header.
/// Format: "{tenantId}|{tenantName}|{userId}|{perm1,perm2,...}"
/// Omit the permissions segment (or leave it empty) to authenticate without permissions.
/// </summary>
public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string IdentityHeader = "X-Test-Identity";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(IdentityHeader, out var headerValues))
            return Task.FromResult(AuthenticateResult.NoResult());

        var header = headerValues.ToString();
        var parts = header.Split('|');

        if (parts.Length < 3)
            return Task.FromResult(AuthenticateResult.NoResult());

        var tenantId = parts[0];
        var tenantName = parts[1];
        var userId = parts[2];

        var permissions = parts.Length > 3 && !string.IsNullOrEmpty(parts[3])
            ? parts[3].Split(',', StringSplitOptions.RemoveEmptyEntries)
            : [];

        var claims = new List<Claim>
        {
            // TenantResolverMiddleware reads AtlasClaims.* to populate IRequestContext
            new(AtlasClaims.TenantId,   tenantId),
            new(AtlasClaims.TenantName, tenantName),
            new(AtlasClaims.UserId,     userId),
            new(AtlasClaims.UserEmail,  "test@acme.com"),
            // UserBootstrapMiddleware skips when this claim is present
            new(AtlasClaims.BootstrapCompleted, "true"),
        };

        foreach (var perm in permissions)
            claims.Add(new Claim(AtlasClaims.Permission, perm));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
