using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Domain.Parties;

/// <summary>
/// A single contact as submitted by a caller replacing a Party's contact collection.
/// Value holds the raw contact string — interpreted according to Type.
/// </summary>
public sealed record ContactInput(ContactType Type, string Value, bool IsPrimary);
