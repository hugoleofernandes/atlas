namespace Atlas.Identity.BffApi.Endpoints.Session.GetSession;

public sealed record GetSessionResponse(
    string Name,
    string Email,
    string UserId
);
