using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Parties.Exceptions;

public sealed class OrganizationNotFoundException : DomainException
{
    public new const string ErrorCode = "party.organization-not-found";

    public OrganizationNotFoundException(Guid partyId)
        : base(ErrorCode, ErrorCategory.NotFound, $"Organization with id '{partyId}' was not found.") { }
}
