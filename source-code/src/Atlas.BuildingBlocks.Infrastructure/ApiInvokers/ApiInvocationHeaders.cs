namespace Atlas.BuildingBlocks.Application.ApiInvokers;

internal static class ApiInvocationHeaders
{
    private static readonly AsyncLocal<Dictionary<string, string?>?> Headers = new();

    public static IReadOnlyDictionary<string, string?> Current => Headers.Value ?? new();

    public static void Set(string key, string? value)
    {
        Headers.Value ??= new Dictionary<string, string?>();
        Headers.Value[key] = value;
    }

    public static void Clear() => Headers.Value = null;
}
