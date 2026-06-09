namespace Atlas.BuildingBlocks.Email.Providers.Resend;

/// <summary>
/// Configuration for the Resend email provider.
/// Bound from <c>appsettings.json</c> section <c>Email:Resend</c>.
///
/// Example:
/// <code>
/// "Email": {
///   "Resend": {
///     "ApiKey": "re_...",
///     "From":   "Atlas &lt;noreply@yourdomain.com&gt;"
///   }
/// }
/// </code>
/// </summary>
public sealed class ResendEmailOptions
{
    public const string SectionName = "Email:Resend";

    /// <summary>Resend API key (starts with <c>re_</c>).</summary>
    public string ApiKey { get; init; } = default!;

    /// <summary>
    /// Sender address in RFC 5322 format.
    /// Examples: <c>"noreply@yourdomain.com"</c> or <c>"Atlas &lt;noreply@yourdomain.com&gt;"</c>.
    /// Must be from a verified domain in your Resend account.
    /// </summary>
    public string From { get; init; } = default!;
}
