using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Exceptions;

public sealed class InvitationAlreadyUsedException : DomainException
{
    public InvitationAlreadyUsedException(string email)
        : base("invitation.already_used", ErrorCategory.Conflict, $"The invitation for '{email}' has already been used.") { }
}
