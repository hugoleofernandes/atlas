namespace Atlas.Outbox.Contracts.Targets;

/// <summary>
/// Explicit mapping for one outbox event target.
/// Step 2 resolves these mappings from a catalog source.
/// Step 3 picks the correct executor by <see cref="Mode"/>.
/// </summary>
public sealed record TargetMapping(
    string Name,
    TargetMode Mode,
    int Order = 0,
    string? Url = null,
    string? Method = null
);
