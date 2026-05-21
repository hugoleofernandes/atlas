using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.ValueObjects.Exceptions;

public sealed class InvalidEmailException : DomainException
{
    public const string ErrorCode = "email.invalid";

    public InvalidEmailException(string email)
        : base(ErrorCode, ErrorCategory.Validation, $"Invalid email format: '{email}'.") { }
}
