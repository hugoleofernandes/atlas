using Atlas.API.Resources;
using Atlas.SharedKernel.Application.Errors;
using Microsoft.Extensions.Localization;

namespace Atlas.API.Errors;

/// <summary>
/// Resolves a localized error message for a given ErrorDefinition.
/// Looks up ErrorDefinition.Code across all context-specific resource files.
/// Falls back to ErrorDefinition.FallbackMessage if no translation is found.
/// Culture is determined automatically from the request's Accept-Language header
/// via RequestLocalizationMiddleware.
/// </summary>
public sealed class ErrorMessageLocalizer : IErrorMessageLocalizer
{
    private readonly IStringLocalizer<SystemErrors> _system;
    private readonly IStringLocalizer<IdentityErrors> _identity;
    private readonly IStringLocalizer<StaffErrors> _staff;

    public ErrorMessageLocalizer(
        IStringLocalizer<SystemErrors> system,
        IStringLocalizer<IdentityErrors> identity,
        IStringLocalizer<StaffErrors> staff)
    {
        _system = system;
        _identity = identity;
        _staff = staff;
    }

    public string Localize(ErrorDefinition error)
    {
        foreach (var localizer in new IStringLocalizer[] { _system, _identity, _staff })
        {
            var result = localizer[error.Code];
            if (!result.ResourceNotFound)
                return result.Value;
        }

        return error.FallbackMessage;
    }
}

