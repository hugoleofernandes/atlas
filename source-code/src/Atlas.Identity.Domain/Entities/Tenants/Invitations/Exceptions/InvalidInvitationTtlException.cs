using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Invitations.Exceptions;

public sealed class InvalidInvitationTtlException : DomainException
{
    public const string ErrorCode = "invitation_ttl.invalid";

    public InvalidInvitationTtlException(TimeSpan ttl)
        : base(ErrorCode, ErrorCategory.Validation, $"Invalid invitation TTL: '{ttl}'.") { }
}
