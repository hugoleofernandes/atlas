using Atlas.BuildingBlocks.Email;
using Atlas.Identity.Contracts.Commands.SendInvitationEmail;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;

namespace Atlas.Identity.Application.Commands.SendInvitationEmail;

/// <summary>
/// Sends the tenant invitation e-mail.
/// Delivery is delegated to <see cref="IEmailService"/> and the provider-level
/// idempotency key is derived from the stable outbox idempotency key.
/// </summary>
public sealed class SendInvitationEmailCommandHandler : ISendInvitationEmailCommandHandler
{
    private readonly IEmailService _emailService;
    private readonly IIdempotencyContext _idempotencyContext;

    public IUnitOfWork UnitOfWork => NullUnitOfWork.Instance;

    public SendInvitationEmailCommandHandler(IEmailService emailService, IIdempotencyContext idempotencyContext)
    {
        _emailService = emailService;
        _idempotencyContext = idempotencyContext;
    }

    public Task<Unit> ExecuteAsync(SendInvitationEmailCommand command, CancellationToken ct) =>
        _emailService
            .SendAsync(BuildMessage(command), ct)
            .ContinueWith(_ => Unit.Value, ct, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);

    private EmailMessage BuildMessage(SendInvitationEmailCommand command)
    {
        string? idempotencyKey =
            _idempotencyContext.IdempotencyKey != Guid.Empty
                ? $"send-invitation-email/{_idempotencyContext.IdempotencyKey}"
                : null;

        return new EmailMessage(
            To: command.Email,
            Subject: "You were invited to Atlas",
            HtmlBody: """
            <h2>You were invited to Atlas</h2>
            <p>An invitation was created for this email address.</p>
            <p>Use your sign-in flow to access the tenant and complete onboarding.</p>
            """,
            IdempotencyKey: idempotencyKey
        );
    }
}
