using Atlas.SharedKernel.Application.Errors;

namespace Atlas.Staff.Application.Errors;

public static class StaffErrors
{
    public static readonly ErrorDefinition AlreadyExists =
        new(
            Code: "STAFF_001",
            DefaultMessage: "Staff already exists",
            Category: ErrorCategory.Conflict
        );
}