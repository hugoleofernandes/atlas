using Atlas.SharedKernel.Domain;

namespace Atlas.Platform.Domain.Modules;

public sealed class Module : AggregateRoot, INotMultiTenant
{
    public Guid   Id       { get; private set; }
    public string Name     { get; private set; } = default!;
    public bool   IsActive { get; private set; }

    private Module() { }

    public static Module Create(string name)
        => new() { Id = Guid.NewGuid(), Name = name, IsActive = true };
}
