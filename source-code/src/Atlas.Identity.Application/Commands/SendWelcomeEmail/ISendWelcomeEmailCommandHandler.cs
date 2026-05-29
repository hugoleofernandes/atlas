using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Commands.SendWelcomeEmail;

/// <summary>
/// Sending a welcome e-mail is a side-effecting command with no database writes.
/// Implements <see cref="ICommandHandler{TCommand,TOutput}"/> with
/// <see cref="NullUnitOfWork"/> so the standard pipeline (PersistDbDecorator)
/// runs without error — SaveChangesAsync is a no-op for this handler.
/// </summary>
public interface ISendWelcomeEmailCommandHandler
    : ICommandHandler<SendWelcomeEmailCommand, Unit>
{
}
