using System.Diagnostics;

namespace Atlas.BuildingBlocks.AspNetCore.Observability;

public static class TraceContextHelper
{
    public static string? GetTraceId()
        => Activity.Current?.TraceId.ToString();
}
