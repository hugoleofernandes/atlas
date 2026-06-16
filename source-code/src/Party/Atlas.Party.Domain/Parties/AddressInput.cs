using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Domain.Parties;

/// <summary>
/// A single address as submitted by a caller replacing a Party's address collection.
/// </summary>
public sealed record AddressInput(AddressType Type, PostalAddress PostalAddress, bool IsPrimary);
