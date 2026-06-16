using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Parties.Exceptions;

public sealed class DuplicateTaxNumberException : DomainException
{
    public new const string ErrorCode = "party.duplicate-tax-number";

    public DuplicateTaxNumberException(string taxNumber)
        : base(ErrorCode, ErrorCategory.Conflict, $"A party with tax number '{taxNumber}' already exists in this tenant.") { }
}
