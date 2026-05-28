namespace Atlas.Identity.API.Endpoints.Auth.Test;

public sealed record GetAuthTestResponse(
    string   Message,
    string[] Tenants,
    string   FrontendBaseUrl
);
