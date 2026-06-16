using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Parties.Exceptions;

public sealed class PartyAlreadyDeactivatedException : DomainException
{
    public new const string ErrorCode = "party.already-deactivated";

    public PartyAlreadyDeactivatedException(Guid partyId)
        : base(ErrorCode, ErrorCategory.Conflict, $"Party '{partyId}' is already deactivated.") { }
}
