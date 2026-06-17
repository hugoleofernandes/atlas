using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Parties.Exceptions;

public sealed class PersonNotFoundException : DomainException
{
    public new const string ErrorCode = "party.person-not-found";

    public PersonNotFoundException(Guid partyId)
        : base(ErrorCode, ErrorCategory.NotFound, $"Person with id '{partyId}' was not found.") { }
}

