using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Shared.Exceptions;

public sealed class InvalidPostalAddressException : DomainException
{
    public new const string ErrorCode = "party.invalid-postal-address";

    public InvalidPostalAddressException(string field)
        : base(ErrorCode, ErrorCategory.Validation, $"Postal address field '{field}' is invalid or missing.") { }
}
