using Atlas.SharedKernel.Application.Errors;
using FluentValidation.Results;

namespace Atlas.BuildingBlocks.Infrastructure.Validation;

/// <summary>
/// Converts FluentValidation results into the application's ErrorDefinition contract,
/// so workflows can return Result.Fail instead of throwing ValidationException.
/// </summary>
public static class ValidationResultExtensions
{
    public static ErrorDefinition ToErrorDefinition(this ValidationResult result)
    {
        var details = string.Join(" | ", result.Errors
            .Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));

        // Code and Category come from the shared catalog.
        // FallbackMessage is overridden with the field-level detail for debugging.
        return new ErrorDefinition(
            Code: CommonErrors.ValidationFailed.Code,
            FallbackMessage: details,
            Category: CommonErrors.ValidationFailed.Category
        );
    }
}
