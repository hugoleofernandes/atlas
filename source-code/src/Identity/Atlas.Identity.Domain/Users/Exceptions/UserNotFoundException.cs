using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Users.Exceptions;

public sealed class UserNotFoundException : DomainException
{
    public const string ErrorCode = "user.not_found";

    public UserNotFoundException(string email)
        : base(ErrorCode, ErrorCategory.NotFound, $"No active user found for '{email}'.") { }
}
