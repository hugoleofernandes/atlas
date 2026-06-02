namespace Atlas.Identity.BffApi.Endpoints.Session.DebugSession;

public sealed record ClaimDto(string Type, string Value);

public sealed record DebugSessionResponse(
    string?              User,
    IEnumerable<ClaimDto> Claims
);
