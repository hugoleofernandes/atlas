using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Commands.SendWelcomeEmail;

/// <summary>
/// Extends <see cref="IHandler{TInput,TOutput}"/> (not ICommandHandler) because
/// sending an e-mail requires no unit-of-work / database commit.
/// </summary>
public interface ISendWelcomeEmailCommandHandler
    : IHandler<SendWelcomeEmailCommand, Unit>
{
}
