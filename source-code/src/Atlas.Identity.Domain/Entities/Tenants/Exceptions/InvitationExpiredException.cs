using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Exceptions;

public sealed class InvitationExpiredException : DomainException
{
    public InvitationExpiredException(string email)
        : base("invitation.expired", ErrorCategory.Business, $"The invitation for '{email}' has expired.") { }
}
