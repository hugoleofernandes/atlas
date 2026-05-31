using Atlas.BuildingBlocks.AuditTrail.Labels;
using Atlas.SharedDomain.Identity;
using Atlas.SharedDomain.Platform;
using Atlas.SharedDomain.Staff;
using Microsoft.Extensions.Localization;

namespace Atlas.SharedDomain.Resources.Audit;

/// <summary>
/// Provides localized audit labels for shared domain concepts:
/// canonical audit actions and deterministic EntityTypeId values.
/// </summary>
public sealed class SharedDomainAuditLabelProvider(
    IStringLocalizer<AuditLabels> localizer
) : IAuditLabelProvider
{
    public string? LocalizeAction(string action)
        => Localize($"audit.action.{action}");

    public string? LocalizeEntityType(Guid entityTypeId)
    {
        var key =
            entityTypeId == IdentityEntityTypes.User       ? "audit.entity.identity.user" :
            entityTypeId == IdentityEntityTypes.Role       ? "audit.entity.identity.role" :
            entityTypeId == IdentityEntityTypes.Invitation ? "audit.entity.identity.invitation" :
            entityTypeId == StaffEntityTypes.StaffMember   ? "audit.entity.staff.staff-member" :
            entityTypeId == PlatformEntityTypes.Tenant     ? "audit.entity.platform.tenant" :
            null;

        return key is null ? null : Localize(key);
    }

    private string? Localize(string key)
    {
        var result = localizer[key];
        return result.ResourceNotFound ? null : result.Value;
    }
}
