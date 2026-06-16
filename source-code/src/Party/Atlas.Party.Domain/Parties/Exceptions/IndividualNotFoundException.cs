using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Parties.Exceptions;

public sealed class IndividualNotFoundException : DomainException
{
    public new const string ErrorCode = "party.individual-not-found";

    public IndividualNotFoundException(Guid partyId)
        : base(ErrorCode, ErrorCategory.NotFound, $"Individual with id '{partyId}' was not found.") { }
}
