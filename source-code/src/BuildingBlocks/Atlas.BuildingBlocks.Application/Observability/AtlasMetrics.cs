using System.Diagnostics.Metrics;

namespace Atlas.BuildingBlocks.Infrastructure.Observability;

public static class AtlasMetrics
{
    private static readonly Meter _meter = new("Atlas", "1.0.0");

    /// <summary>
    /// Counts new users created from invitations.
    /// Tags: tenant.name
    /// </summary>
    public static readonly Counter<long> UsersCreated =
        _meter.CreateCounter<long>(
            "atlas.users.created",
            unit: "{user}",
            description: "Total number of users created from invitations");
}
