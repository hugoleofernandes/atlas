using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Exceptions;

public sealed class InvitationNotFoundException : DomainException
{
    public const string ErrorCode = "invitation.not_found";

    public InvitationNotFoundException(string email)
        : base(ErrorCode, ErrorCategory.NotFound, $"No invitation found for email '{email}'.") { }
}
