namespace Atlas.API.Models.Session;

public sealed record GetSessionResponse(
    string Name,
    string Email,
    string UserId
);
