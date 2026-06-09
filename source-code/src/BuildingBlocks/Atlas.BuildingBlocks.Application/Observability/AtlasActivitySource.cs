using System.Diagnostics;

namespace Atlas.BuildingBlocks.Infrastructure.Observability;

/// <summary>
/// Shared ActivitySource for all Atlas instrumentation.
/// Register with OTel SDK via .AddSource("Atlas").
/// </summary>
public static class AtlasActivitySource
{
    public static readonly ActivitySource Source = new("Atlas", "1.0.0");
}
