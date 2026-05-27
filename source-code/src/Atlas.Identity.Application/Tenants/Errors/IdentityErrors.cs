using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.Entities.Tenants.Invitations.Exceptions;
using Atlas.Identity.Domain.Entities.Tenants.Users.Exceptions;
using Atlas.Identity.Domain.Exceptions;
using Atlas.SharedKernel.Application.Errors;

namespace Atlas.Identity.Application.Tenants.Errors;

/// <summary>
/// Error catalog for the Identity module.
/// Codes reference the ErrorCode constants defined in domain exceptions,
/// ensuring a single source of truth — changing the code in the exception
/// automatically updates the catalog and the .resx lookup key.
/// </summary>
public static class IdentityErrors
{
    public static class Tenant
    {
        public static readonly ErrorDefinition NotFound = new(
            Code: TenantNotFoundException.ErrorCode,
            FallbackMessage: "Tenant not found.",
            Category: ErrorCategory.NotFound
        );

        public static readonly ErrorDefinition Inactive = new(
            Code: TenantInactiveException.ErrorCode,
            FallbackMessage: "Tenant is inactive.",
            Category: ErrorCategory.Business
        );
    }

    public static class User
    {
        public static readonly ErrorDefinition AlreadyExists = new(
            Code: UserAlreadyExistsException.ErrorCode,
            FallbackMessage: "A user with this email already exists.",
            Category: ErrorCategory.Conflict
        );
    }

    public static class Invitation
    {
        public static readonly ErrorDefinition NotFound = new(
            Code: InvitationNotFoundException.ErrorCode,
            FallbackMessage: "Invitation not found.",
            Category: ErrorCategory.NotFound
        );

        public static readonly ErrorDefinition Expired = new(
            Code: InvitationExpiredException.ErrorCode,
            FallbackMessage: "This invitation has expired.",
            Category: ErrorCategory.Business
        );

        public static readonly ErrorDefinition AlreadyUsed = new(
            Code: InvitationAlreadyUsedException.ErrorCode,
            FallbackMessage: "This invitation has already been used.",
            Category: ErrorCategory.Conflict
        );

        public static readonly ErrorDefinition Duplicate = new(
            Code: DuplicateInvitationException.ErrorCode,
            FallbackMessage: "An active invitation for this email already exists.",
            Category: ErrorCategory.Conflict
        );
    }
}
