using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Exceptions;

public sealed class InvitationNotFoundException : DomainException
{
    public InvitationNotFoundException(string email)
        : base($"No invitation found for email '{email}'.") { }
}