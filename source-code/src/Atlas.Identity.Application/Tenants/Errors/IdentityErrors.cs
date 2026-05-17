using Atlas.SharedKernel.Application.Errors;

namespace Atlas.Identity.Application.Tenants.Errors;

/// <summary>
/// Error catalog for the Identity module.
/// Codes follow the pattern: "{entity}.{snake_case_reason}"
/// These codes are also used as i18n keys in ErrorMessages.resx.
/// </summary>
public static class IdentityErrors
{
    public static class Tenant
    {
        public static readonly ErrorDefinition NotFound = new(
            Code: "tenant.not_found",
            FallbackMessage: "Tenant not found.",
            Category: ErrorCategory.NotFound
        );

        public static readonly ErrorDefinition Inactive = new(
            Code: "tenant.inactive",
            FallbackMessage: "Tenant is inactive.",
            Category: ErrorCategory.Business
        );
    }

    public static class User
    {
        public static readonly ErrorDefinition AlreadyExists = new(
            Code: "user.already_exists",
            FallbackMessage: "A user with this email already exists.",
            Category: ErrorCategory.Conflict
        );
    }

    public static class Invitation
    {
        public static readonly ErrorDefinition NotFound = new(
            Code: "invitation.not_found",
            FallbackMessage: "Invitation not found.",
            Category: ErrorCategory.NotFound
        );

        public static readonly ErrorDefinition Expired = new(
            Code: "invitation.expired",
            FallbackMessage: "This invitation has expired.",
            Category: ErrorCategory.Business
        );

        public static readonly ErrorDefinition AlreadyUsed = new(
            Code: "invitation.already_used",
            FallbackMessage: "This invitation has already been used.",
            Category: ErrorCategory.Conflict
        );

        public static readonly ErrorDefinition Duplicate = new(
            Code: "invitation.duplicate",
            FallbackMessage: "An active invitation for this email already exists.",
            Category: ErrorCategory.Conflict
        );
    }
}
