using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.BuildingBlocks.AspNetCore.Security.InternalApi;

public sealed class InternalApiKeyAuthenticationHandler(
    IOptionsMonitor<InternalApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<InternalApiKeyAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (string.IsNullOrWhiteSpace(Options.ApiKey))
            return Task.FromResult(AuthenticateResult.Fail("Internal API key is not configured."));

        if (!Request.Headers.TryGetValue(Options.HeaderName, out var values))
            return Task.FromResult(AuthenticateResult.NoResult());

        var actual = values.ToString();
        if (string.IsNullOrWhiteSpace(actual))
            return Task.FromResult(AuthenticateResult.Fail("Internal API key header is empty."));

        if (!IsValidApiKey(actual, Options.ApiKey))
            return Task.FromResult(AuthenticateResult.Fail("Invalid internal API key."));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Options.ServiceName),
            new Claim(ClaimTypes.Name, Options.ServiceName),
            new Claim(InternalApiKeyDefaults.ActorTypeClaim, InternalApiKeyDefaults.ServiceActorType),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static bool IsValidApiKey(string actual, string expected)
    {
        var actualBytes = Encoding.UTF8.GetBytes(actual);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return actualBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
    }
}
