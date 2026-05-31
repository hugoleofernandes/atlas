namespace Atlas.BuildingBlocks.AuditTrail.Labels;

public interface IAuditLabelProvider
{
    string? LocalizeAction(string action);

    string? LocalizeEntityType(Guid entityTypeId);
}
