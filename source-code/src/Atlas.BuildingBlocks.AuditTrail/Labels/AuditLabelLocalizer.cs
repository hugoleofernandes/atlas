namespace Atlas.BuildingBlocks.AuditTrail.Labels;

public sealed class AuditLabelLocalizer(IEnumerable<IAuditLabelProvider> providers)
{
    public string LocalizeAction(string action)
        => providers
               .Select(provider => provider.LocalizeAction(action))
               .FirstOrDefault(label => label is not null)
           ?? action;

    public string LocalizeEntityType(Guid entityTypeId)
        => providers
               .Select(provider => provider.LocalizeEntityType(entityTypeId))
               .FirstOrDefault(label => label is not null)
           ?? entityTypeId.ToString();
}
