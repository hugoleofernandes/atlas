using Atlas.SharedKernel.Application.Errors;

namespace Atlas.Staff.Application.Errors;

/// <summary>
/// Error catalog for the Staff module.
/// Codes follow the pattern: "{entity}.{snake_case_reason}"
/// These codes are also used as i18n keys in ErrorMessages.resx.
/// </summary>
public static class StaffErrors
{
    public static class StaffMember
    {
        public static readonly ErrorDefinition AlreadyExists = new(
            Code: "staff_member.already_exists",
            FallbackMessage: "A staff member with this email already exists.",
            Category: ErrorCategory.Conflict
        );
    }
}
