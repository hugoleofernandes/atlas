using Atlas.SharedKernel.Domain;

namespace Atlas.Platform.Domain.Geography;

/// <summary>
/// Represents a city within a state. Seeded once; never modified after creation.
/// StateId is the FK to the owning State — country/state codes are resolved via JOIN in readers.
/// </summary>
public sealed class City : AggregateRoot, INotMultiTenant
{
    public Guid   Id       { get; private set; }
    public Guid   StateId  { get; private set; }
    public string Name     { get; private set; } = default!;
    public bool   IsActive { get; private set; }

    private City() { }

    public static City Create(Guid stateId, string name)
        => new()
        {
            Id      = Guid.NewGuid(),
            StateId = stateId,
            Name    = name,
            IsActive = true,
        };
}
