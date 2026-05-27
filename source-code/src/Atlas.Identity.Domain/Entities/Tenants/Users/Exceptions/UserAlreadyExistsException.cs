using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Users.Exceptions;

public sealed class UserAlreadyExistsException : DomainException
{
    public const string ErrorCode = "user.already_exists";

    public UserAlreadyExistsException(string email)
        : base(ErrorCode, ErrorCategory.Conflict, $"A user with email '{email}' already exists in this tenant.") { }
}
