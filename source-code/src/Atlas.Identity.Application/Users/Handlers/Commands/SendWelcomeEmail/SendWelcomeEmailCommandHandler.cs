using Atlas.BuildingBlocks.Email;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;

namespace Atlas.Identity.Application.Users.Handlers.Commands.SendWelcomeEmail;

/// <summary>
/// Sends a welcome email to a user who completed registration via an invitation link.
///
/// Delegates delivery to <see cref="IEmailService"/> — the handler has no knowledge
/// of the underlying provider (Resend, SendGrid, etc.) or any fallback strategy.
///
/// Idempotency: when invoked from the outbox worker, <see cref="IIdempotencyContext"/>
/// carries a stable GUID (same across retries). That GUID is forwarded to the email
/// provider so it deduplicates on its end — even if the outbox retries the message,
/// the user receives exactly one welcome email.
///
/// No database writes — PersistDbDecorator calls SaveChangesAsync safely as a no-op.
/// </summary>
public sealed class SendWelcomeEmailCommandHandler : ISendWelcomeEmailCommandHandler
{
    private readonly IEmailService       _emailService;
    private readonly IIdempotencyContext _idempotencyContext;

    /// <inheritdoc/>
    public IUnitOfWork UnitOfWork => NullUnitOfWork.Instance;

    public SendWelcomeEmailCommandHandler(
        IEmailService       emailService,
        IIdempotencyContext idempotencyContext)
    {
        _emailService       = emailService;
        _idempotencyContext = idempotencyContext;
    }

    public Task<Unit> ExecuteAsync(SendWelcomeEmailCommand command, CancellationToken ct)
        => _emailService
            .SendAsync(BuildMessage(command), ct)
            .ContinueWith(_ => Unit.Value, ct, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);

    // ── Private ──────────────────────────────────────────────────────────────

    private EmailMessage BuildMessage(SendWelcomeEmailCommand command)
    {
        // Build a stable idempotency key using the outbox message's IdempotencyKey (a GUID
        // that never changes across retry attempts). Follows Resend's recommended format:
        // <event-type>/<entity-id>.
        // Guard against Guid.Empty — which means the handler is running outside an outbox
        // context (tests, direct invocation) where no idempotency context is available.
        string? idempotencyKey = _idempotencyContext.IdempotencyKey != Guid.Empty
            ? $"send-welcome-email/{_idempotencyContext.IdempotencyKey}"
            : null;

        return new EmailMessage(
            To:             command.Email,
            Subject:        "Welcome to Atlas",
            HtmlBody:       $"""
                <h2>Welcome to Atlas!</h2>
                <p>Your account is ready. You can now sign in and start using the platform.</p>
                <p>If you have any questions, just reply to this email.</p>
                """,
            IdempotencyKey: idempotencyKey);
    }
}
