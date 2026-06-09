namespace Atlas.BuildingBlocks.Email;

/// <summary>
/// Sends transactional emails via the configured provider.
///
/// Callers have no knowledge of which provider is active (Resend, SendGrid, etc.)
/// or whether a fallback strategy is in place — that is entirely the concern of
/// the registered implementation.
///
/// Future extension point: register a <c>FallbackEmailService</c> that wraps
/// multiple <c>IEmailProvider</c> implementations and retries with the next
/// provider on failure — without changing a single call site.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends the email message.
    /// Throws on unrecoverable delivery failure so the outbox worker can retry.
    /// </summary>
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
