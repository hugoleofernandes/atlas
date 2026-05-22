using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Application.Tenants.Commands.SendWelcomeEmail;

/// <summary>
/// Sends a welcome email to a user who completed registration via an invitation link.
///
/// Pure application logic — has no knowledge of how this command was triggered.
/// The trigger (OutboxWorker, API, test harness) is the adapter's concern.
///
/// TODO: inject IEmailService when the email infrastructure is implemented.
/// </summary>
public sealed class SendWelcomeEmailCommandHandler : ISendWelcomeEmailCommandHandler
{
    private readonly ILogger<SendWelcomeEmailCommandHandler> _logger;

    /// <inheritdoc/>
    /// No database writes — PersistDbDecorator calls SaveChangesAsync safely as a no-op.
    public IUnitOfWork UnitOfWork => NullUnitOfWork.Instance;

    public SendWelcomeEmailCommandHandler(
        ILogger<SendWelcomeEmailCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task<Unit> ExecuteAsync(SendWelcomeEmailCommand command, CancellationToken ct)
    {
        // TODO: send welcome email via IEmailService
        _logger.LogInformation(
            "Welcome email queued — TenantId={TenantId} UserId={UserId}",
            command.TenantId, command.UserId);

        return Task.FromResult(Unit.Value);
    }
}
