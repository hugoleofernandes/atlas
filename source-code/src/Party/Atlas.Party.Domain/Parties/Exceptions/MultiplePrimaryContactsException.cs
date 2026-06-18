using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Parties.Exceptions;

public sealed class MultiplePrimaryContactsException : DomainException
{
    public new const string ErrorCode = "party.multiple-primary-contacts";

    public MultiplePrimaryContactsException(string channel)
        : base(ErrorCode, ErrorCategory.Validation, $"Only one primary {channel} contact is allowed.") { }
}
