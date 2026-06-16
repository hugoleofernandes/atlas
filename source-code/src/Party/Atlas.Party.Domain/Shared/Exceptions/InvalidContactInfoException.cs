using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Shared.Exceptions;

public sealed class InvalidContactInfoException : DomainException
{
    public new const string ErrorCode = "party.invalid-contact-info";

    public InvalidContactInfoException(string type, string raw)
        : base(ErrorCode, ErrorCategory.Validation, $"'{raw}' is not a valid {type}.") { }
}
