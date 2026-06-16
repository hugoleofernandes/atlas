using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Shared.Exceptions;

public sealed class InvalidTaxNumberException : DomainException
{
    public new const string ErrorCode = "party.invalid-tax-number";

    public InvalidTaxNumberException(string raw)
        : base(ErrorCode, ErrorCategory.Validation, $"'{raw}' is not a valid CPF or CNPJ.") { }
}
