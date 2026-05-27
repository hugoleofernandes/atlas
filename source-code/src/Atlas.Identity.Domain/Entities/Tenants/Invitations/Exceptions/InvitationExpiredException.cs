using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Invitations.Exceptions;

public sealed class InvitationExpiredException : DomainException
{
    public const string ErrorCode = "invitation.expired";

    public InvitationExpiredException(string email)
        : base(ErrorCode, ErrorCategory.Business, $"The invitation for '{email}' has expired.") { }
}
