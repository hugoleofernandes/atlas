using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants.Exceptions;

public sealed class DuplicateInvitationException : DomainException
{
    public DuplicateInvitationException(string email)
        : base($"An active invitation already exists for email '{email}'.") { }
}