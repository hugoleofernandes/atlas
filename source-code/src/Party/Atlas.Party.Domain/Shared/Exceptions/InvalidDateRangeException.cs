using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Shared.Exceptions;

public sealed class InvalidDateRangeException : DomainException
{
    public new const string ErrorCode = "party.invalid-date-range";

    public InvalidDateRangeException(DateOnly start, DateOnly end)
        : base(ErrorCode, ErrorCategory.Validation, $"Date range end '{end}' cannot be before start '{start}'.") { }
}
