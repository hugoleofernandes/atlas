using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Commands.SendInvitationEmail;

/// <summary>
/// Sends the invitation e-mail after an invitation is created.
/// No database writes - uses <see cref="NullUnitOfWork"/>.
/// </summary>
public interface ISendInvitationEmailCommandHandler : ICommandHandler<SendInvitationEmailCommand, Unit> { }
