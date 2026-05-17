using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Exceptions;

public sealed class InvitationNotFoundException : DomainException
{
    public InvitationNotFoundException(string email)
        : base("invitation.not_found", ErrorCategory.NotFound, $"No invitation found for email '{email}'.") { }
}
