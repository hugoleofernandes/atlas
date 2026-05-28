using Atlas.Identity.Resources.Auth;
using Atlas.Identity.Resources.Common;
using Atlas.Identity.Resources.Invitations;
using Atlas.Identity.Resources.Tenants;
using Atlas.Identity.Resources.Users;
using Atlas.SharedKernel.Application.Errors;
using Atlas.Staff.Resources.StaffMember;
using Microsoft.Extensions.Localization;

namespace Atlas.API.Errors;

/// <summary>
/// Resolves a localized error message for a given ErrorDefinition.
/// Looks up ErrorDefinition.Code across all domain-specific resource files.
/// Falls back to ErrorDefinition.FallbackMessage if no translation is found.
/// Culture is determined automatically from the request's Accept-Language header
/// via RequestLocalizationMiddleware.
/// </summary>
public sealed class ErrorMessageLocalizer : IErrorMessageLocalizer
{
    private readonly IStringLocalizer[] _localizers;

    public ErrorMessageLocalizer(
        IStringLocalizer<SystemErrors>     system,
        IStringLocalizer<TenantErrors>     tenant,
        IStringLocalizer<UserErrors>       user,
        IStringLocalizer<InvitationErrors> invitation,
        IStringLocalizer<ClaimErrors>      claim,
        IStringLocalizer<StaffMemberErrors>      staff)
    {
        _localizers =
        [
            system,
            tenant,
            user,
            invitation,
            claim,
            staff,
        ];
    }

    public string Localize(ErrorDefinition error)
    {
        foreach (var localizer in _localizers)
        {
            var result = localizer[error.Code];
            if (!result.ResourceNotFound)
                return result.Value;
        }

        return error.FallbackMessage;
    }
}
