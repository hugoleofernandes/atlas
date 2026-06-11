using Atlas.BuildingBlocks.Email;
using Atlas.Identity.Application.Emails;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Idempotency;
using Microsoft.Extensions.Options;

namespace Atlas.Identity.Application.Commands.SendWelcomeEmail;

public sealed class SendWelcomeEmailCommandHandler(
    IEmailService emailService,
    IIdempotencyContext idempotencyContext,
    IOptions<IdentityEmailOptions> emailOptions) : ISendWelcomeEmailCommandHandler
{
    public IUnitOfWork UnitOfWork => NullUnitOfWork.Instance;

    public async Task<Unit> ExecuteAsync(SendWelcomeEmailCommand command, CancellationToken ct)
    {
        await emailService.SendAsync(BuildMessage(command), ct);
        return Unit.Value;
    }

    private EmailMessage BuildMessage(SendWelcomeEmailCommand command)
    {
        string? idempotencyKey =
            idempotencyContext.IdempotencyKey != Guid.Empty
                ? $"send-welcome-email/{idempotencyContext.IdempotencyKey}"
                : null;

        var (subject, htmlBody) = IdentityEmailTemplates.Welcome(emailOptions.Value.LoginUrl);

        return new EmailMessage(
            To: command.Email,
            Subject: subject,
            HtmlBody: htmlBody,
            IdempotencyKey: idempotencyKey
        );
    }
}
