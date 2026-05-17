using Atlas.API.Resources;
using Atlas.SharedKernel.Application.Errors;
using Microsoft.Extensions.Localization;

namespace Atlas.API.Errors;

/// <summary>
/// Resolves a localized error message for a given ErrorDefinition.
/// Looks up ErrorDefinition.Code in the resource files (ErrorMessages.resx / ErrorMessages.pt.resx).
/// Falls back to ErrorDefinition.FallbackMessage if no translation is found.
/// Culture is determined automatically from the request's Accept-Language header
/// via RequestLocalizationMiddleware.
/// </summary>
public sealed class ErrorMessageLocalizer
{
    private readonly IStringLocalizer<ErrorMessages> _localizer;

    public ErrorMessageLocalizer(IStringLocalizer<ErrorMessages> localizer)
    {
        _localizer = localizer;
    }

    public string Localize(ErrorDefinition error)
    {
        var localized = _localizer[error.Code];
        return localized.ResourceNotFound
            ? error.FallbackMessage
            : localized.Value;
    }
}
