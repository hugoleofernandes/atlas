using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Shared.Exceptions;

public sealed class InvalidPersonNameException : DomainException
{
    public new const string ErrorCode = "party.invalid-person-name";

    public InvalidPersonNameException(string field)
        : base(ErrorCode, ErrorCategory.Validation, $"Person name field '{field}' is required and cannot be empty.") { }
}
