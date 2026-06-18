using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Parties.Exceptions;

public sealed class DuplicatePartyClassificationTypeException : DomainException
{
    public const string ErrorCode = "party_classification.duplicate_type";

    public DuplicatePartyClassificationTypeException(string type)
        : base(ErrorCode, ErrorCategory.Conflict, $"Duplicate classification type '{type}'.") { }
}
