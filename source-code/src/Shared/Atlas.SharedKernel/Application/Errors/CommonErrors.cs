namespace Atlas.SharedKernel.Application.Errors;

/// <summary>
/// Cross-cutting error definitions shared across all modules.
/// </summary>
public static class CommonErrors
{
    public static readonly ErrorDefinition Unexpected = new(
        Code: "common.unexpected",
        FallbackMessage: "An unexpected error occurred.",
        Category: ErrorCategory.Unexpected
    );

    public static readonly ErrorDefinition ValidationFailed = new(
        Code: "validation.failed",
        FallbackMessage: "Validation failed.",
        Category: ErrorCategory.Validation
    );
}
