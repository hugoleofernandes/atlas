namespace Atlas.BuildingBlocks.Email;

/// <summary>
/// Represents a transactional email to be sent via <see cref="IEmailService"/>.
///
/// The sender address is owned by configuration (<c>Email:Resend:From</c>) —
/// individual messages never specify it, keeping the "from" identity consistent
/// across all outgoing emails without coupling callers to infrastructure details.
/// </summary>
public sealed record EmailMessage(
    /// <summary>Recipient email address.</summary>
    string  To,

    /// <summary>Subject line.</summary>
    string  Subject,

    /// <summary>HTML body — primary rendering for modern clients.</summary>
    string  HtmlBody,

    /// <summary>
    /// Optional plain-text fallback for clients that don't render HTML.
    /// When null the provider renders its own plain-text version from the HTML.
    /// </summary>
    string? PlainTextBody  = null,

    /// <summary>
    /// Optional idempotency key forwarded to the email provider.
    ///
    /// Use format <c>&lt;event-type&gt;/&lt;stable-id&gt;</c>, for example
    /// <c>"send-welcome-email/3fa85f64-..."</c>.
    ///
    /// When set, the provider deduplicates retries server-side for 24 hours — a request
    /// sent with the same key more than once delivers the email only once.
    /// Should be derived from the outbox <c>IdempotencyKey</c> (stable across attempts)
    /// so retries from the outbox worker are automatically deduplicated end-to-end.
    ///
    /// Leave null when called outside an outbox context (tests, one-off sends).
    /// </summary>
    string? IdempotencyKey = null
);
