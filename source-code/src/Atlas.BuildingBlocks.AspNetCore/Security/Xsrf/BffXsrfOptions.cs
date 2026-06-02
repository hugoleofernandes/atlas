using Microsoft.AspNetCore.Http;

namespace Atlas.BuildingBlocks.AspNetCore.Security.Xsrf;

public sealed class BffXsrfOptions
{
    public string BffPathPrefix { get; set; } = BffXsrfDefaults.BffPathPrefix;
    public HashSet<PathString> ExcludedPaths { get; } =
    [
        new(BffXsrfDefaults.FakeLoginPath),
    ];
}
