using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Invitations.Exceptions;

public sealed class InvitationAlreadyUsedException : DomainException
{
    public const string ErrorCode = "invitation.already_used";

    public InvitationAlreadyUsedException(string email)
        : base(ErrorCode, ErrorCategory.Conflict, $"The invitation for '{email}' has already been used.") { }
}
