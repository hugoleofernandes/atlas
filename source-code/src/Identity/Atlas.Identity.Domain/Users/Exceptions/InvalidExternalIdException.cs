using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Users.Exceptions;

public sealed class InvalidExternalIdException : DomainException
{
    public const string ErrorCode = "external_id.invalid";

    public InvalidExternalIdException(string value)
        : base(ErrorCode, ErrorCategory.Validation, $"Invalid external identity provider user identifier: '{value}'.") { }
}
