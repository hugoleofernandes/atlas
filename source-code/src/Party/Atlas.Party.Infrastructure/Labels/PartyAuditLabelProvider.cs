using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.Party.Contracts.EntityTypes;

namespace Atlas.Party.Infrastructure.Labels;

/// <summary>
/// Provides audit labels for Party module entity types.
/// Action localization is handled by a shared provider.
/// </summary>
public sealed class PartyAuditLabelProvider : IAuditLabelProvider
{
    public string? LocalizeAction(string action) => null;

    public string? LocalizeEntityType(Guid entityTypeId)
    {
        if (entityTypeId == PartyModuleEntityTypes.Persons.EntityType.Id)
            return "Person";
        if (entityTypeId == PartyModuleEntityTypes.Organizations.EntityType.Id)
            return "Organization";
        return null;
    }
}
