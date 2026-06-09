using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Users.Exceptions;

/// <summary>
/// Thrown when the ExternalId (OID) in the incoming token does not match
/// the ExternalId stored for the existing user with that email.
/// This indicates a different identity provider account is trying to access
/// a user profile that belongs to another account.
/// </summary>
public sealed class UserIdentityMismatchException : DomainException
{
    public const string ErrorCode = "user.identity_mismatch";

    public UserIdentityMismatchException(string email)
        : base(ErrorCode, ErrorCategory.Unauthorized,
            $"The identity token does not match the registered account for '{email}'.") { }
}
