using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Domain.Parties.Exceptions;

public sealed class MultiplePrimaryAddressesException : DomainException
{
    public new const string ErrorCode = "party.multiple-primary-addresses";

    public MultiplePrimaryAddressesException(AddressType type)
        : base(ErrorCode, ErrorCategory.Validation, $"Only one primary address of type '{type}' is allowed.") { }
}
