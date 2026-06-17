using Atlas.SharedKernel.Domain;

namespace Atlas.Platform.Domain.Geography;

/// <summary>
/// Represents a country subdivision (state, province, district) used as reference data
/// for address forms. Seeded once per country; never modified after creation.
/// </summary>
public sealed class State : AggregateRoot, INotMultiTenant
{
    public Guid   Id          { get; private set; }
    public string CountryCode { get; private set; } = default!;
    public string Code        { get; private set; } = default!;
    public string Name        { get; private set; } = default!;
    public bool   IsActive    { get; private set; }

    private State() { }

    public static State Create(Guid id, string countryCode, string code, string name)
        => new()
        {
            Id          = id,
            CountryCode = countryCode.ToUpperInvariant(),
            Code        = code.ToUpperInvariant(),
            Name        = name,
            IsActive    = true,
        };
}
