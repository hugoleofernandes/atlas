using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants.Exceptions;

public sealed class UserAlreadyExistsException : DomainException
{
    public UserAlreadyExistsException(string email)
        : base($"A user with email '{email}' already exists in this tenant.") { }
}