using System.Security.Cryptography;
using System.Text;
using Hangfire.AspNetCore;
using Hangfire.Dashboard;

namespace Atlas.Outbox.Infrastructure.Hangfire;

public sealed class HangfireDashboardBasicAuthFilter(
    string username,
    string password,
    bool allowInsecureHttp) : IDashboardAuthorizationFilter
{
    private readonly byte[] _usernameBytes = Encoding.UTF8.GetBytes(username);
    private readonly byte[] _passwordBytes = Encoding.UTF8.GetBytes(password);

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (!allowInsecureHttp && !httpContext.Request.IsHttps)
            return false;

        if (!httpContext.Request.Headers.TryGetValue("Authorization", out var headerValues))
            return false;

        var header = headerValues.ToString();
        if (!header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return false;

        var encodedCredentials = header["Basic ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(encodedCredentials))
            return false;

        string decodedCredentials;
        try
        {
            decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
        }
        catch (FormatException)
        {
            return false;
        }

        var separatorIndex = decodedCredentials.IndexOf(':');
        if (separatorIndex < 0)
            return false;

        var providedUsername = Encoding.UTF8.GetBytes(decodedCredentials[..separatorIndex]);
        var providedPassword = Encoding.UTF8.GetBytes(decodedCredentials[(separatorIndex + 1)..]);

        return CryptographicOperations.FixedTimeEquals(providedUsername, _usernameBytes)
            && CryptographicOperations.FixedTimeEquals(providedPassword, _passwordBytes);
    }
}
