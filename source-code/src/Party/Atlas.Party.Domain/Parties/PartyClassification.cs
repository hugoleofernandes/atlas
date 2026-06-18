using Atlas.Party.Domain.Shared;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Parties;

/// <summary>
/// Records the role a Party plays in the business (e.g. Staff, Customer).
/// Supports vigência: active between Since and Until (open-ended when Until is null).
/// At most one classification per ClassificationType per Party.
/// </summary>
public sealed class PartyClassification : AuditableEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid PartyId { get; private set; }

    public ClassificationType Type { get; private set; }

    public DateOnly Since { get; private set; }

    public DateOnly? Until { get; private set; }

    private PartyClassification() { }

    internal static PartyClassification Create(Guid partyId, ClassificationType type, DateOnly since, DateOnly? until)
        => new() { PartyId = partyId, Type = type, Since = since, Until = until };
}
