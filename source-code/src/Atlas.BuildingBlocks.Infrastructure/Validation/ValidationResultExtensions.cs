using Atlas.SharedKernel.Application.Errors;
using FluentValidation;
using FluentValidation.Results;

namespace Atlas.BuildingBlocks.Infrastructure.Validation;

/// <summary>
/// Converts FluentValidation results into the application's ErrorDefinition contract.
/// </summary>
public static class ValidationResultExtensions
{
    public static ErrorDefinition ToErrorDefinition(this ValidationResult result)
    {
        var details = string.Join(" | ", result.Errors
            .Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));

        return new ErrorDefinition(
            Code: CommonErrors.ValidationFailed.Code,
            FallbackMessage: details,
            Category: CommonErrors.ValidationFailed.Category
        );
    }

    public static ErrorDefinition ToErrorDefinition(this ValidationException ex)
    {
        var details = string.Join(" | ", ex.Errors
            .Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));

        return new ErrorDefinition(
            Code: CommonErrors.ValidationFailed.Code,
            FallbackMessage: details,
            Category: CommonErrors.ValidationFailed.Category
        );
    }
}
