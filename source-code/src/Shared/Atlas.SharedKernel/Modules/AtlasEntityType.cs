namespace Atlas.SharedKernel.Modules;

public readonly record struct AtlasEntityType(Guid Id, string Name, AtlasModule Module)
{
    public static AtlasEntityType Create(string entitySuffix, string name, AtlasModule module)
    {
        var guidText = $"00000000-0000-0000-{module.Code:D4}-{entitySuffix.PadLeft(12, '0')}";
        return new AtlasEntityType(new Guid(guidText), name, module);
    }
}
