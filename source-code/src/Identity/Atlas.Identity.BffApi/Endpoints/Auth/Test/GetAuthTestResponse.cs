namespace Atlas.Identity.BffApi.Endpoints.Auth.Test;

public sealed record GetAuthTestResponse(
    string   Message,
    string[] Tenants,
    string   FrontendBaseUrl
);
