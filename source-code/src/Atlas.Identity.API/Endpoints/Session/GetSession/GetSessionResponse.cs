namespace Atlas.Identity.API.Endpoints.Session.GetSession;

public sealed record GetSessionResponse(
    string Name,
    string Email,
    string UserId
);
