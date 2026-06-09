namespace Atlas.BuildingBlocks.Audit.Labels;

public interface IAuditLabelProvider
{
    string? LocalizeAction(string action);

    string? LocalizeEntityType(Guid entityTypeId);
}
