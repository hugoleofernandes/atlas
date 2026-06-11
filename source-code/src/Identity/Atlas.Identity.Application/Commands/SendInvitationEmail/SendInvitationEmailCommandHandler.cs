using Atlas.BuildingBlocks.Email;
using Atlas.Identity.Application.Emails;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Idempotency;
using Microsoft.Extensions.Options;

namespace Atlas.Identity.Application.Commands.SendInvitationEmail;

public sealed class SendInvitationEmailCommandHandler(
    IEmailService emailService,
    IIdempotencyContext idempotencyContext,
    IOptions<IdentityEmailOptions> emailOptions) : ISendInvitationEmailCommandHandler
{
    public IUnitOfWork UnitOfWork => NullUnitOfWork.Instance;

    public async Task<Unit> ExecuteAsync(SendInvitationEmailCommand command, CancellationToken ct)
    {
        await emailService.SendAsync(BuildMessage(command), ct);
        return Unit.Value;
    }

    private EmailMessage BuildMessage(SendInvitationEmailCommand command)
    {
        string? idempotencyKey =
            idempotencyContext.IdempotencyKey != Guid.Empty
                ? $"send-invitation-email/{idempotencyContext.IdempotencyKey}"
                : null;

        var (subject, htmlBody) = IdentityEmailTemplates.Invitation(emailOptions.Value.LoginUrl);

        return new EmailMessage(
            To: command.Email,
            Subject: subject,
            HtmlBody: htmlBody,
            IdempotencyKey: idempotencyKey
        );
    }
}
