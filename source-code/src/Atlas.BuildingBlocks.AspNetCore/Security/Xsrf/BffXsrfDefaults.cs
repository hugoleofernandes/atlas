namespace Atlas.BuildingBlocks.AspNetCore.Security.Xsrf;

public static class BffXsrfDefaults
{
    public const string HeaderName = "X-XSRF-TOKEN";
    public const string CookieName = "__Host-atlas-xsrf";
    public const string BffPathPrefix = "/bff";
    public const string FakeLoginPath = "/bff/identity/dev/login-fake";
    public const string ErrorCode = "security.xsrf_invalid";
}
