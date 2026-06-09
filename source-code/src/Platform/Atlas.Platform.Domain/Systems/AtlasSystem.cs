using Atlas.SharedKernel.Domain;

namespace Atlas.Platform.Domain.Systems;

public sealed class AtlasSystem : AggregateRoot, INotMultiTenant
{
    public Guid   Id       { get; private set; }
    public string Name     { get; private set; } = default!;
    public bool   IsActive { get; private set; }

    private AtlasSystem() { }

    public static AtlasSystem Create(string name)
        => new() { Id = Guid.NewGuid(), Name = name, IsActive = true };
}
