using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;
using ResendMessage = Resend.EmailMessage;

namespace Atlas.BuildingBlocks.Email.Providers.Resend;

/// <summary>
/// <see cref="IEmailService"/> implementation backed by the Resend API.
///
/// Resend delivers email via a simple REST API — no SMTP, no credentials rotation.
/// API key is read from <see cref="ResendEmailOptions"/> (bound from appsettings).
///
/// On failure the exception propagates to the caller (outbox worker), which records
/// the failure and schedules a retry — no silent swallowing here.
/// </summary>
internal sealed class ResendEmailService : IEmailService
{
    private readonly IResend                     _resend;
    private readonly ResendEmailOptions          _options;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(
        IResend                          resend,
        IOptions<ResendEmailOptions>     options,
        ILogger<ResendEmailService>      logger)
    {
        _resend  = resend;
        _options = options.Value;
        _logger  = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var resendMessage = new ResendMessage
        {
            From     = _options.From,
            To       = { message.To },
            Subject  = message.Subject,
            HtmlBody = message.HtmlBody,
        };

        if (message.PlainTextBody is not null)
            resendMessage.TextBody = message.PlainTextBody;

        _logger.LogInformation(
            "Sending email via Resend — To={To} Subject={Subject}",
            message.To, message.Subject);

        // When an idempotency key is provided the Resend API deduplicates retries
        // server-side for 24 hours — safe to retry without sending the email twice.
        // EmailSendAsync overload accepts the key as a plain string.
        ResendResponse<Guid> response = message.IdempotencyKey is not null
            ? await _resend.EmailSendAsync(message.IdempotencyKey, resendMessage, ct)
            : await _resend.EmailSendAsync(resendMessage, ct);

        _logger.LogInformation(
            "Email sent via Resend — To={To} MessageId={MessageId}",
            message.To, response.Content);
    }
}
