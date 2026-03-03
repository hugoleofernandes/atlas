using System.Diagnostics;

namespace Atlas.API.Observability;

public static class TraceContextHelper
{
    public static string? GetTraceId()
    {
        return Activity.Current?.TraceId.ToString();
    }
}