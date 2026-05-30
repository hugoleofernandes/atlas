namespace Atlas.Platform.Application.Queries.EntityTypes.Lookup;

public sealed record EntityTypeLookupDto(
    Guid   EntityTypeId,
    string EntityTypeName,
    Guid   ModuleId,
    string ModuleName);
