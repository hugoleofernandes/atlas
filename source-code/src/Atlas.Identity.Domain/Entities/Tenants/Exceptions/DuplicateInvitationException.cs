using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Exceptions;

public sealed class DuplicateInvitationException : DomainException
{
    public const string ErrorCode = "invitation.duplicate";

    public DuplicateInvitationException(string email)
        : base(ErrorCode, ErrorCategory.Conflict, $"An active invitation already exists for email '{email}'.") { }
}
